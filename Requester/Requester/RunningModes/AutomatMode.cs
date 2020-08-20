using Newtonsoft.Json;
using Requester.Data;
using Requester.Requests;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Requester.RunningModes
{
    public class AutomatMode
    {
        private readonly HttpClient httpClient;
        private readonly Settings settings;
        private readonly AutomatSettings automatSettings;
        private readonly SessionRequester sessionRequester;
        private readonly ILogger logger;

        public AutomatMode(HttpClient httpClient, Settings settings, AutomatSettings automatSettings, SessionRequester sessionRequester, ILogger logger)
        {
            this.httpClient = httpClient;
            this.settings = settings;
            this.automatSettings = automatSettings;
            this.sessionRequester = sessionRequester;
            this.logger = logger;
        }

        public void Perform()
        {
            Parallel.For(0, settings.Threads, i => Perform(i));
        }

        private void Perform(int index)
        {
            var currentTime = automatSettings.StartTime;
            while (currentTime < automatSettings.EndTime)
            {
                var scenarioTimer = Stopwatch.StartNew();
                var scenarioId = Guid.NewGuid().ToString();

                var scenarioPartTimer = Stopwatch.StartNew();
                var token = sessionRequester.GetToken("automat", scenarioId);
                logger.Information($"Service='Automat' ScenarioId='{scenarioId}' Method='automat token' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");

                scenarioPartTimer.Restart();
                var data = GetData(index, scenarioId, currentTime);
                logger.Information($"Service='Automat' ScenarioId='{scenarioId}' Method='automat getbatch' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");

                var toPay = data.Payments.Where(p => p.LatestProcessingTimestamp + p.Interval < DateTime.UtcNow);

                var balancesDict = data.Balances.ToDictionary(k => k.Id, v => v);
                var loansDict = data.Loans.ToDictionary(k => k.PaymentId, v => v);

                var withSufficientBalance = toPay.Where(p =>
                {
                    if (p.Amount <= balancesDict[p.AccountId].Amount)
                    {
                        balancesDict[p.AccountId].Amount -= p.Amount;
                        return true;
                    }
                    else { return false; };
                }).ToArray();
                var withInsufficientBalance = toPay.Except(withSufficientBalance);
                var paidIds = withSufficientBalance.Select(p => p.Id).ToHashSet();

                var transfers = withSufficientBalance.Select(p => CreateTransfer(p, loansDict.ContainsKey(p.Id) ? loansDict[p.Id] : null)).ToArray();
                var messages = withInsufficientBalance.Select(p => CreateMessage(p, balancesDict[p.AccountId])).ToArray();
                var repaidInstalments = data.Loans.Where(l => paidIds.Contains(l.PaymentId)).Select(l => l.Id).ToArray();
                var processedIds = toPay.Select(p => p.Id).ToArray();

                var batchProcess = new BatchProcess { ProcessingTimestamp = currentTime, Transfers = transfers, Messages = messages, RepaidInstalmentsIds = repaidInstalments, ProcessedPaymentsIds = processedIds };

                scenarioPartTimer.Restart();
                SendData(batchProcess, scenarioId);
                logger.Information($"Service='Automat' ScenarioId='{scenarioId}' Method='automat processbatch' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");

                scenarioPartTimer.Restart();
                sessionRequester.Logout(token, scenarioId);
                logger.Information($"Service='Automat' ScenarioId='{scenarioId}' Method='automat logout' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");

                logger.Information($"Service='Automat' ScenarioId='{scenarioId}' Method='automat scenario' Processing='{scenarioTimer.ElapsedMilliseconds}'");

                //Thread.Sleep(automatSettings.SleepTime);
                //currentTime += scenarioTimer.Elapsed;
                currentTime += TimeSpan.FromMinutes(5);
            }
        }

        private void SendData(BatchProcess batchProcess, string scenarioId)
        {
            var body = JsonConvert.SerializeObject(batchProcess);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            content.Headers.Add("flowId", scenarioId);
            var result = httpClient.PostAsync("batch", content).Result;
        }

        private BatchData GetData(int index, string scenarioId, DateTime dateTime)
        {
            var url = $"Batch?part={index + 1}&total={automatSettings.TotalCount}&timestamp={dateTime.ToString("s", CultureInfo.InvariantCulture)}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("flowId", scenarioId);
            var result = httpClient.SendAsync(request).Result;
            var response = result.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<BatchData>(response);
        }

        private AccountTransfer CreateTransfer(PaymentDTO payment, LoanDTO loan)
        {
            var title = loan != null ? CreateLoanTitle(loan) : $"Transaction of payment {payment.Id}";
            return new AccountTransfer
            {
                AccountId = payment.AccountId,
                PaymentId = payment.Id,
                Amount = payment.Amount,
                Recipient = payment.Recipient,
                Title = title
            };
        }

        private string CreateLoanTitle(LoanDTO loan)
        {
            var currentlyPaid = (loan.PaidAmount / loan.TotalAmount) * loan.Instalments;
            return $"{currentlyPaid + 1} of {loan.Instalments} instalments. Payment {loan.PaymentId}";
        }

        private MessageDTO CreateMessage(PaymentDTO payment, BalanceDTO balance)
        {
            var content = $"There is insufficient balance on account {balance.Id} to repay payment {payment.Id}. Required balance: {payment.Amount}, current balance: {balance.Amount}";
            return new MessageDTO { UserId = balance.UserId, Content = content };
        }
    }
}

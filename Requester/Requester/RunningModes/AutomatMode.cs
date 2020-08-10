﻿using Jil;
using Requester.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Requester.RunningModes
{
    public class AutomatMode
    {
        private readonly HttpClient httpClient;
        private readonly Settings settings;
        private readonly AutomatSettings automatSettings;

        public AutomatMode(HttpClient httpClient, Settings settings, AutomatSettings automatSettings)
        {
            this.httpClient = httpClient;
            this.settings = settings;
            this.automatSettings = automatSettings;
        }

        public void Perform()
        {
            Parallel.For(0, settings.Threads, i => Perform(i));
        }

        private void Perform(int index)
        {
            var currentTime = automatSettings.CurrentDate;
            var data = GetData(index);
            var toPay = data.Payments.Where(p => ShouldBePaid(p));

            var balancesDict = data.Balances.ToDictionary(k => k.Id, v => v);
            var loansDict = data.Loans.ToDictionary(k => k.PaymentId, v => v);

            var withSufficientBalance = toPay.Where(p => p.Amount <= balancesDict[p.AccountId].Amount).ToArray();
            var withInsufficientBalance = toPay.Except(withSufficientBalance);
            var paidIds = withSufficientBalance.Select(p => p.Id).ToHashSet();

            var transfers = withSufficientBalance.Select(p => CreateTransfer(p, loansDict.ContainsKey(p.Id) ? loansDict[p.Id] : null)).ToArray();
            var messages = withInsufficientBalance.Select(p => CreateMessage(p, balancesDict[p.AccountId])).ToArray();
            var repaidInstalments = data.Loans.Where(l => paidIds.Contains(l.PaymentId)).Select(l => l.Id).ToArray();
        }

        private bool ShouldBePaid(PaymentDTO payment, DateTime currentTime)
        {

        }

        private BatchData GetData(int index)
        {
            var url = $"Batch?part={index}&total={automatSettings.TotalCount}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var scenarioId = Guid.NewGuid().ToString();
            request.Headers.Add("flowId", scenarioId);
            var result = httpClient.SendAsync(request).Result;
            var response = result.Content.ReadAsStringAsync().Result;
            return JSON.Deserialize<BatchData>(response);
        }

        private AccountTransfer CreateTransfer(PaymentDTO payment, LoanDTO loan)
        {
            var title = loan != null ? CreateLoanTitle(loan) : $"Transaction of payment {payment.Id}";
            return new AccountTransfer
            {
                AccountId = payment.AccountId,
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
            return new MessageDTO { UserId = "", Content = content };
        }
    }
}

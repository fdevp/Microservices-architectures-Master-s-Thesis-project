using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Requester.RunningModes
{
    public class SetupMode
    {
        private readonly HttpClient httpClient;
        private readonly ILogger logger;

        public SetupMode(HttpClient httpClient, ILogger logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public void Perform()
        {
            var setup = File.ReadAllText("setup.json");
            var response = httpClient.PostAsync($"setup/setup", new StringContent(setup, Encoding.UTF8, "application/json")).Result;
            if (!response.IsSuccessStatusCode)
            {
                logger.Information("Cannot setup system.");
                return;
            }
            logger.Information("General setup done.");

            if (File.Exists("accounts.json"))
            {
                var accounts = JsonConvert.DeserializeObject<AccountsSetup>(File.ReadAllText("accounts.json")).Accounts;
                for (int i = 0; i < accounts.Length; i += 10000)
                {
                    var portion = accounts.Skip(i).Take(10000);
                    var accountsSetup = new AccountsSetup { Accounts = portion.ToArray() };
                    try
                    {
                        var result = httpClient.PostAsync("http://localhost:5000/account/setup", new StringContent(JsonConvert.SerializeObject(accountsSetup), Encoding.UTF8, "application/json")).Result;
                    }
                    catch (Exception e)
                    {
                        int asd = 5;
                    }

                    logger.Information("Accounts portion setup done.");
                }
            }

            if (File.Exists("transactions.json"))
            {
                var transactions = JsonConvert.DeserializeObject<TransactionsSetup>(File.ReadAllText("transactions.json")).Transactions;
                for (int i = 0; i < transactions.Length; i += 1000)
                {
                    var portion = transactions.Skip(i).Take(1000);
                    var transactionsSetp = new TransactionsSetup { Transactions = portion.ToArray() };
                    var result = httpClient.PostAsync("transaction/setup", new StringContent(JsonConvert.SerializeObject(transactionsSetp), Encoding.UTF8, "application/json")).Result;
                    logger.Information("Transactions portion setup done.");
                }
            }

            if (File.Exists("payments.json"))
            {

                var payments = JsonConvert.DeserializeObject<PaymentsSetup>(File.ReadAllText("payments.json")).Payments;
                for (int i = 0; i < payments.Length; i += 10000)
                {
                    var portion = payments.Skip(i).Take(10000);
                    var paymentsSetup = new PaymentsSetup { Payments = portion.ToArray() };
                    var result = httpClient.PostAsync("payment/setup", new StringContent(JsonConvert.SerializeObject(paymentsSetup), Encoding.UTF8, "application/json")).Result;
                    logger.Information("Payments portion setup done.");
                }
            }


            if (File.Exists("loans.json"))
            {
                var loans = JsonConvert.DeserializeObject<LoansSetup>(File.ReadAllText("loans.json")).Loans;
                for (int i = 0; i < loans.Length; i += 10000)
                {
                    var portion = loans.Skip(i).Take(10000);
                    var loansSetup = new LoansSetup { Loans = portion.ToArray() };
                    var result = httpClient.PostAsync("loan/setup", new StringContent(JsonConvert.SerializeObject(loansSetup), Encoding.UTF8, "application/json")).Result;
                    logger.Information("Loans portion setup done.");
                }
            }


            logger.Information("Setup all done.");
        }
    }
}

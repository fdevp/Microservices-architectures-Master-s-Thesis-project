using Jil;
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

        public async Task Perform()
        {
            var setup = File.ReadAllText("setup.json");
            var response = await httpClient.PostAsync($"setup/setup", new StringContent(setup, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                logger.Information("Cannot setup system.");
                return;
            }

            logger.Information("General setup done.");

            var transactions = JSON.Deserialize<TransactionsSetup>(File.ReadAllText("transactions.json")).Transactions;
            for (int i = 0; i < transactions.Length; i += 10000)
            {
                var portion = transactions.Skip(i).Take(10000);
                var transactionsSetp = new TransactionsSetup { Transactions = portion.ToArray() };
                await httpClient.PostAsync("http://localhost:5000/transaction/setup", new StringContent(JSON.Serialize(transactionsSetp), Encoding.UTF8, "application/json"));
                logger.Information("Transactions portion setup done.");
            }

            logger.Information("Setup all done.");
        }
    }
}

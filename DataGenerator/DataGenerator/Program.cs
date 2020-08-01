using DataGenerator.DTO;
using DataGenerator.Rnd;
using Jil;
using System;
using System.IO;
using System.Linq;

namespace DataGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var setupall = DataGenerator.Generate(10000, new DateTime(2015, 1, 1), new DateTime(2020, 8, 1));
            var transactionsSetup = setupall.TransactionsSetup;
            setupall.TransactionsSetup = new TransactionsSetup { Transactions = new TransactionDTO[0] };

            //File.WriteAllText("setup.json", JSON.Serialize(setupall));
            //File.WriteAllText("transactions.json", JSON.Serialize(transactionsSetup));

            //var scenario = ScenarioGenerator.IndividualUserScenario(setupall, 16, 50);
            //var scenario = ScenarioGenerator.BusinessUserScenario(setupall, 16, 1, 5);
            //var scenario = ScenarioGenerator.UserActivityReportsScenario(setupall, 0, 1, new DateTime(2015, 1, 1), new DateTime(2020, 8, 1));
            var scenario = ScenarioGenerator.OverallActivityReportScenario(10, 100, new DateTime(2015, 1, 1), new DateTime(2020, 8, 1));
        }
    }
}

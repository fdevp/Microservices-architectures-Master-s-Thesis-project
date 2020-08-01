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
            var setupall = DataGenerator.Generate(1000, new DateTime(2015, 1, 1), new DateTime(2020, 8, 1));
            var transactionsSetup = setupall.TransactionsSetup;
            setupall.TransactionsSetup = new TransactionsSetup { Transactions = new TransactionDTO[0] };

            File.WriteAllText("setup.json", JSON.Serialize(setupall));
            File.WriteAllText("transactions.json", JSON.Serialize(transactionsSetup));

            var individualScenario = ScenarioGenerator.IndividualUserScenario(setupall, 16, 50);
            File.WriteAllText("individual.json", individualScenario);

            var businessScenario = ScenarioGenerator.BusinessUserScenario(setupall, 16, 1, 5);
            File.WriteAllText("business.json", businessScenario);

            var userActivityReportScenario = ScenarioGenerator.UserActivityReportsScenario(setupall, 1, 5, new DateTime(2015, 1, 1), new DateTime(2020, 8, 1));
            File.WriteAllText("userActivityReportScenario.json", userActivityReportScenario);

            var overallReportScenario = ScenarioGenerator.OverallReportScenario(10, 100, new DateTime(2015, 1, 1), new DateTime(2020, 8, 1));
            File.WriteAllText("overallReportScenario.json", overallReportScenario);
        }
    }
}

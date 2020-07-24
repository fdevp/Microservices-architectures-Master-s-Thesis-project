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
            var setupall = Generator.Generate();
            var transactionsSetup = setupall.TransactionsSetup;
            setupall.TransactionsSetup = new TransactionsSetup { Transactions = new TransactionDTO[0] };

            File.WriteAllText("setup.json", JSON.Serialize(setupall));
            File.WriteAllText("transactions.json", JSON.Serialize(transactionsSetup));

            var scenario = ScenarioGenerator.GenerateIndividualUser(setupall);
        }
    }
}

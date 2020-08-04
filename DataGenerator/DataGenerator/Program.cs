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
            var businessUsersSetup = BusinessUserDataGenerator.Generate(200, new DateTime(2015, 1, 1), new DateTime(2020, 8, 1));
            var individualUsersSetup = IndividualUserDataGenerator.Generate(1000, new DateTime(2015, 1, 1), new DateTime(2020, 8, 1));
            var additionalUsers = new UserDTO[2] {
                new UserDTO { Id = Guid.NewGuid().ToString(), Login = "analyst", Password = "password" },
                new UserDTO { Id = Guid.NewGuid().ToString(), Login = "automat", Password = "password" }
            };

            var transactionsSetup = new TransactionsSetup { Transactions = businessUsersSetup.TransactionsSetup.Transactions.Concat(individualUsersSetup.TransactionsSetup.Transactions).ToArray() };
            var setupAll = new SetupAll
            {
                UsersSetup = new UsersSetup { Users = businessUsersSetup.UsersSetup.Users.Concat(individualUsersSetup.UsersSetup.Users).Concat(additionalUsers).ToArray() },
                AccountsSetup = new AccountsSetup { Accounts = businessUsersSetup.AccountsSetup.Accounts.Concat(individualUsersSetup.AccountsSetup.Accounts).ToArray() },
                CardsSetup = new CardsSetup { Cards = businessUsersSetup.CardsSetup.Cards.Concat(individualUsersSetup.CardsSetup.Cards).ToArray() },
                LoansSetup = new LoansSetup { Loans = businessUsersSetup.LoansSetup.Loans.Concat(individualUsersSetup.LoansSetup.Loans).ToArray() },
                PaymentsSetup = new PaymentsSetup { Payments = businessUsersSetup.PaymentsSetup.Payments.Concat(individualUsersSetup.PaymentsSetup.Payments).ToArray() },
                TransactionsSetup = new TransactionsSetup { Transactions = new TransactionDTO[0] }
            };

            File.WriteAllText("setup.json", JSON.Serialize(setupAll));
            File.WriteAllText("transactions.json", JSON.Serialize(transactionsSetup));

            var individualScenario = ScenarioGenerator.IndividualUserScenario(setupAll, 16, 5000);
            File.WriteAllText("individual.json", individualScenario);

            var businessScenario = ScenarioGenerator.BusinessUserScenario(setupAll, 16, 1000, 3, 7);
            File.WriteAllText("business.json", businessScenario);

            var userActivityReportScenario = ScenarioGenerator.UserActivityReportsScenario(setupAll, 1, 5, new DateTime(2015, 1, 1), new DateTime(2020, 8, 1));
            File.WriteAllText("userActivityReportScenario.json", userActivityReportScenario);

            var overallReportScenario = ScenarioGenerator.OverallReportScenario(10, 100, new DateTime(2015, 1, 1), new DateTime(2020, 8, 1));
            File.WriteAllText("overallReportScenario.json", overallReportScenario);
        }
    }
}

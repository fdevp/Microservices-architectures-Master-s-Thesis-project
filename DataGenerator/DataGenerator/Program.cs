﻿using DataGenerator.DTO;
using DataGenerator.Generators;
using DataGenerator.Rnd;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace DataGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            //var automatSetup = AutomatDataGenerator.Generate(20000, new DateTime(2020, 8, 1, 0,0,0), new DateTime(2020, 8, 1, 1, 0, 0));
            //AddSpecialUsers(automatSetup);
            //File.WriteAllText("loans.json", JsonConvert.SerializeObject(automatSetup.LoansSetup));
            //File.WriteAllText("accounts.json", JsonConvert.SerializeObject(automatSetup.AccountsSetup));
            //File.WriteAllText("payments.json", JsonConvert.SerializeObject(automatSetup.PaymentsSetup));

            //automatSetup.AccountsSetup = new AccountsSetup { Accounts = new AccountDTO[0] };
            //automatSetup.LoansSetup = new LoansSetup { Loans = new LoanDTO[0] };
            //automatSetup.PaymentsSetup = new PaymentsSetup { Payments = new PaymentDTO[0] };
            //File.WriteAllText("setup.json", JsonConvert.SerializeObject(automatSetup));


            
            var businessUsersSetup = BusinessUserDataGenerator.Generate(200, new DateTime(2015, 1, 1), new DateTime(2020, 8, 1));
            var individualUsersSetup = IndividualUserDataGenerator.Generate(1000, new DateTime(2015, 1, 1), new DateTime(2020, 8, 1));
            

            var transactionsSetup = businessUsersSetup.TransactionsSetup.Concat(individualUsersSetup.TransactionsSetup);
            businessUsersSetup.TransactionsSetup = new TransactionsSetup();
            individualUsersSetup.TransactionsSetup = new TransactionsSetup();

            var setupAll = businessUsersSetup.Concat(individualUsersSetup);
            AddSpecialUsers(setupAll);

            File.WriteAllText("setup.json", JsonConvert.SerializeObject(setupAll));
            File.WriteAllText("transactions.json", JsonConvert.SerializeObject(transactionsSetup));

            var individualScenario = ScenarioGenerator.IndividualUserScenario(setupAll, 16, 5000);
            File.WriteAllText("individual.json", individualScenario);

            var businessScenario = ScenarioGenerator.BusinessUserScenario(setupAll, 16, 1000, 3, 7);
            File.WriteAllText("business.json", businessScenario);

            var userActivityReportScenario = ScenarioGenerator.UserActivityReportsScenario(setupAll, 1, 5, new DateTime(2015, 1, 1), new DateTime(2020, 8, 1));
            File.WriteAllText("userActivityReportScenario.json", userActivityReportScenario);

            var overallReportScenario = ScenarioGenerator.OverallReportScenario(10, 100, new DateTime(2015, 1, 1), new DateTime(2020, 8, 1));
            File.WriteAllText("overallReportScenario.json", overallReportScenario);
        }

        private static SetupAll AddSpecialUsers(SetupAll setup)
        {
            var additionalUsers = new UserDTO[2] {
                new UserDTO { Id = Guid.NewGuid().ToString(), Login = "analyst", Password = "password" },
                new UserDTO { Id = Guid.NewGuid().ToString(), Login = "automat", Password = "password" }
            };

            setup.UsersSetup.Users = additionalUsers.Concat(setup.UsersSetup.Users).ToArray();
            return setup;
        }
    }
}

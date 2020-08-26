using DataGenerator.DTO;
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
            //GenerateIndividualScenario();
            //GenerateBusinessScenario();
            //GenerateReportsScenario();
            //GenerateAutomatScenario();
            //GenerateReportsScenario();

            //GenerateFirstAdditional();
            //GenerateSecondAdditional();
            //GenerateThirdAdditional();
            GenerateFourthAdditional();
        }

        private static void GenerateFourthAdditional()
        {
            var setupBusiness = BusinessUserDataGenerator.Generate(500, new DateTime(2010, 1, 1), new DateTime(2020, 8, 1));
            AddSpecialUsers(setupBusiness);
            
            var businessScenario = ScenarioGenerator.BusinessUserScenario(setupBusiness, 16, 500, 5, 5, 2);
            for (int i = 0; i < businessScenario.Length; i++)
                File.WriteAllText(i + "business.json", businessScenario[i]);

            var setupReports = ReportDataGenerator.GenerateOverallReportData(3000, 40000, 40000, 40000, 40000, new DateTime(2010, 1, 1), new DateTime(2020, 8, 1));
            
            var userActivityReportScenario = ScenarioGenerator.UserActivityReportsScenario(setupReports, 1, 1, new DateTime(2010, 1, 1), new DateTime(2020, 8, 1), 2);
            for (int i = 0; i < userActivityReportScenario.Length; i++)
                File.WriteAllText(i + "userActivityReportScenario.json", userActivityReportScenario[i]);

            var overallReportScenario = ScenarioGenerator.OverallReportScenario(1000, 1000, new DateTime(2010, 1, 1), new DateTime(2020, 8, 1), TimeSpan.FromDays(1050), 2);
            for (int i = 0; i < overallReportScenario.Length; i++)
                File.WriteAllText(i + "overallReportScenario.json", overallReportScenario[i]);

            var setupAll = setupBusiness.Concat(setupReports);
            File.WriteAllText("transactions.json", JsonConvert.SerializeObject(setupAll.TransactionsSetup));

            setupAll.TransactionsSetup = new TransactionsSetup();
            File.WriteAllText("setup.json", JsonConvert.SerializeObject(setupAll));
        }

        private static void GenerateThirdAdditional()
        {
            var reportSetup = ReportDataGenerator.GenerateOverallReportData(10000, 50000, 50000, 50000, 50000, new DateTime(2008, 1, 1), new DateTime(2018, 8, 1));
            var userActivityReportScenario = ScenarioGenerator.UserActivityReportsScenario(reportSetup, 1, 1, new DateTime(2008, 1, 1), new DateTime(2018, 8, 1), 2);
            for (int i = 0; i < userActivityReportScenario.Length; i++)
                File.WriteAllText(i + "userActivityReportScenario.json", userActivityReportScenario[i]);

            var overallReportScenario = ScenarioGenerator.OverallReportScenario(3500, 3500, new DateTime(2008, 1, 1), new DateTime(2018, 8, 1), TimeSpan.FromDays(1050), 2);
            for (int i = 0; i < overallReportScenario.Length; i++)
                File.WriteAllText(i + "overallReportScenario.json", overallReportScenario[i]);

            var automatSetup = AutomatDataGenerator.Generate(30000, new DateTime(2020, 8, 1, 0, 0, 0), new DateTime(2020, 8, 1, 0, 15, 0));
            var setupAll = reportSetup.Concat(automatSetup);
            File.WriteAllText("loans.json", JsonConvert.SerializeObject(setupAll.LoansSetup));
            File.WriteAllText("accounts.json", JsonConvert.SerializeObject(setupAll.AccountsSetup));
            File.WriteAllText("payments.json", JsonConvert.SerializeObject(setupAll.PaymentsSetup));
            File.WriteAllText("transactions.json", JsonConvert.SerializeObject(setupAll.TransactionsSetup));

            setupAll.AccountsSetup = new AccountsSetup { Accounts = new AccountDTO[0] };
            setupAll.LoansSetup = new LoansSetup { Loans = new LoanDTO[0] };
            setupAll.PaymentsSetup = new PaymentsSetup { Payments = new PaymentDTO[0] };
            setupAll.TransactionsSetup = new TransactionsSetup { Transactions = new TransactionDTO[0] };

            File.WriteAllText("setup.json", JsonConvert.SerializeObject(setupAll));
        }

        private static void GenerateSecondAdditional()
        {
            var invidiualSetup = IndividualUserDataGenerator.Generate(2000, new DateTime(2018, 1, 1), new DateTime(2020, 7, 1));
            AddSpecialUsers(invidiualSetup);

            invidiualSetup.LoansSetup = new LoansSetup();
            invidiualSetup.PaymentsSetup = new PaymentsSetup();
            invidiualSetup.TransactionsSetup = new TransactionsSetup();
            var individualScenario = ScenarioGenerator.IndividualUserScenario(invidiualSetup, 16, 29000, 2);
            for (int i = 0; i < individualScenario.Length; i++)
                File.WriteAllText(i + "individual.json", individualScenario[i]);

            var automatSetup = AutomatDataGenerator.Generate(40000, new DateTime(2020, 8, 1, 0, 0, 0), new DateTime(2020, 8, 1, 0, 15, 0));
            File.WriteAllText("loans.json", JsonConvert.SerializeObject(automatSetup.LoansSetup));
            File.WriteAllText("accounts.json", JsonConvert.SerializeObject(automatSetup.AccountsSetup));
            File.WriteAllText("payments.json", JsonConvert.SerializeObject(automatSetup.PaymentsSetup));

            automatSetup.AccountsSetup = new AccountsSetup { Accounts = new AccountDTO[0] };
            automatSetup.LoansSetup = new LoansSetup { Loans = new LoanDTO[0] };
            automatSetup.PaymentsSetup = new PaymentsSetup { Payments = new PaymentDTO[0] };

            var setupAll = invidiualSetup.Concat(automatSetup);
            File.WriteAllText("setup.json", JsonConvert.SerializeObject(setupAll));
        }

        private static void GenerateFirstAdditional()
        {
            var invidiualSetup = IndividualUserDataGenerator.Generate(2000, new DateTime(2018, 1, 1), new DateTime(2020, 8, 1));
            AddSpecialUsers(invidiualSetup);
            invidiualSetup.LoansSetup = new LoansSetup();
            invidiualSetup.PaymentsSetup = new PaymentsSetup();
            invidiualSetup.TransactionsSetup = new TransactionsSetup();
            var individualScenario = ScenarioGenerator.IndividualUserScenario(invidiualSetup, 8, 12000, 1);
            for (int i = 0; i < individualScenario.Length; i++)
                File.WriteAllText(i + "individual.json", individualScenario[i]);
            
            var reportSetup = ReportDataGenerator.GenerateOverallReportData(10000, 50000, 50000, 50000, 50000, new DateTime(2008, 1, 1), new DateTime(2018, 8, 1));
            var userActivityReportScenario = ScenarioGenerator.UserActivityReportsScenario(reportSetup, 1, 1, new DateTime(2008, 1, 1), new DateTime(2018, 8, 1), 2);
            for (int i = 0; i < userActivityReportScenario.Length; i++)
                File.WriteAllText(i + "userActivityReportScenario.json", userActivityReportScenario[i]);

            var overallReportScenario = ScenarioGenerator.OverallReportScenario(3500, 3500, new DateTime(2008, 1, 1), new DateTime(2018, 8, 1), TimeSpan.FromDays(1050), 2);
            for (int i = 0; i < overallReportScenario.Length; i++)
                File.WriteAllText(i + "overallReportScenario.json", overallReportScenario[i]);

            var setupAll = invidiualSetup.Concat(reportSetup);
            var transactionsSetup = setupAll.TransactionsSetup;
            setupAll.TransactionsSetup = new TransactionsSetup();
            File.WriteAllText("setup.json", JsonConvert.SerializeObject(setupAll));
            File.WriteAllText("transactions.json", JsonConvert.SerializeObject(transactionsSetup));
        }

        private static void GenerateAutomatScenario()
        {
            var automatSetup = AutomatDataGenerator.Generate(60000, new DateTime(2020, 8, 1, 0, 0, 0), new DateTime(2020, 8, 1, 0, 15, 0));
            AddSpecialUsers(automatSetup);
            File.WriteAllText("loans.json", JsonConvert.SerializeObject(automatSetup.LoansSetup));
            File.WriteAllText("accounts.json", JsonConvert.SerializeObject(automatSetup.AccountsSetup));
            File.WriteAllText("payments.json", JsonConvert.SerializeObject(automatSetup.PaymentsSetup));

            automatSetup.AccountsSetup = new AccountsSetup { Accounts = new AccountDTO[0] };
            automatSetup.LoansSetup = new LoansSetup { Loans = new LoanDTO[0] };
            automatSetup.PaymentsSetup = new PaymentsSetup { Payments = new PaymentDTO[0] };
            File.WriteAllText("setup.json", JsonConvert.SerializeObject(automatSetup));
        }

        private static void GenerateIndividualScenario()
        {
            var setupAll = IndividualUserDataGenerator.Generate(2000, new DateTime(2015, 1, 1), new DateTime(2020, 8, 1));
            AddSpecialUsers(setupAll);
            File.WriteAllText("transactions.json", JsonConvert.SerializeObject(setupAll.TransactionsSetup));
            setupAll.TransactionsSetup = new TransactionsSetup();
            File.WriteAllText("setup.json", JsonConvert.SerializeObject(setupAll));

            var individualScenario = ScenarioGenerator.IndividualUserScenario(setupAll, 48, 15000, 6);
            for (int i = 0; i < individualScenario.Length; i++)
                File.WriteAllText(i + "individual.json", individualScenario[i]);
        }

        private static void GenerateBusinessScenario()
        {
            var setupAll = BusinessUserDataGenerator.Generate(500, new DateTime(2015, 1, 1), new DateTime(2020, 8, 1));
            AddSpecialUsers(setupAll);
            File.WriteAllText("transactions.json", JsonConvert.SerializeObject(setupAll.TransactionsSetup));
            setupAll.TransactionsSetup = new TransactionsSetup();
            File.WriteAllText("setup.json", JsonConvert.SerializeObject(setupAll));

            var businessScenario = ScenarioGenerator.BusinessUserScenario(setupAll, 16, 500, 5, 5, 2);
            for (int i = 0; i < businessScenario.Length; i++)
                File.WriteAllText(i + "business.json", businessScenario[i]);
        }

        private static void GenerateBusinessAndIndividualScenario()
        {
            var businessUsersSetup = BusinessUserDataGenerator.Generate(500, new DateTime(2015, 1, 1), new DateTime(2020, 8, 1));
            var individualUsersSetup = IndividualUserDataGenerator.Generate(500, new DateTime(2015, 1, 1), new DateTime(2020, 8, 1));


            var transactionsSetup = businessUsersSetup.TransactionsSetup.Concat(individualUsersSetup.TransactionsSetup);
            businessUsersSetup.TransactionsSetup = new TransactionsSetup();
            individualUsersSetup.TransactionsSetup = new TransactionsSetup();

            var setupAll = businessUsersSetup.Concat(individualUsersSetup);
            AddSpecialUsers(setupAll);

            File.WriteAllText("setup.json", JsonConvert.SerializeObject(setupAll));
            File.WriteAllText("transactions.json", JsonConvert.SerializeObject(transactionsSetup));

            var individualScenario = ScenarioGenerator.IndividualUserScenario(setupAll, 16, 2000, 2);
            for (int i = 0; i < individualScenario.Length; i++)
                File.WriteAllText(i + "individual.json", individualScenario[i]);

            var businessScenario = ScenarioGenerator.BusinessUserScenario(setupAll, 16, 2000, 3, 7, 2);
            for (int i = 0; i < businessScenario.Length; i++)
                File.WriteAllText(i + "business.json", businessScenario[i]);

            /*
            var userActivityReportScenario = ScenarioGenerator.UserActivityReportsScenario(setupAll, 1, 5, new DateTime(2015, 1, 1), new DateTime(2020, 8, 1));
            File.WriteAllText("userActivityReportScenario.json", userActivityReportScenario);

            var overallReportScenario = ScenarioGenerator.OverallReportScenario(10, 100, new DateTime(2015, 1, 1), new DateTime(2020, 8, 1));
            File.WriteAllText("overallReportScenario.json", overallReportScenario);*/
        }

        private static void GenerateReportsScenario()
        {
            var setupAll = ReportDataGenerator.GenerateOverallReportData(12000, 50000, 50000, 50000, 50000, new DateTime(2010, 1, 1), new DateTime(2020, 8, 1));
            AddSpecialUsers(setupAll);

            var transactionsSetup = setupAll.TransactionsSetup;
            setupAll.TransactionsSetup = new TransactionsSetup();
            File.WriteAllText("setup.json", JsonConvert.SerializeObject(setupAll));
            File.WriteAllText("transactions.json", JsonConvert.SerializeObject(transactionsSetup));

            var userActivityReportScenario = ScenarioGenerator.UserActivityReportsScenario(setupAll, 1, 1, new DateTime(2010, 1, 1), new DateTime(2020, 8, 1), 2);
            for(int i=0;i<userActivityReportScenario.Length;i++)
                File.WriteAllText(i+"userActivityReportScenario.json", userActivityReportScenario[i]);

            var overallReportScenario = ScenarioGenerator.OverallReportScenario(5000, 5000, new DateTime(2010, 1, 1), new DateTime(2020, 8, 1), TimeSpan.FromDays(1050), 2);
            for (int i = 0; i < overallReportScenario.Length; i++)
                File.WriteAllText(i + "overallReportScenario.json", overallReportScenario[i]);
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

using DataGenerator.DTO;
using DataGenerator.Rnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGenerator
{
    public static class DataGenerator
    {
        public static SetupAll Generate(int usersCount, DateTime minDate, DateTime maxDate)
        {
            var users = ValuesGenerator.CreateUsers(usersCount).ToArray();
            var accounts = Accounts(users);
            var recipientRnd = new RndBuilder<string>().DistributionValues(accounts.Select(a => a.Id)).Build();
            var timestampRnd = new RndBuilder<DateTime>().Min(minDate).Max(maxDate).Build();

            var cards = Cards(accounts);
            var activePayments = ActivePayments(accounts, recipientRnd, timestampRnd);
            var loansAndPayments = ActiveLoans(accounts, recipientRnd);
            
            /*
            var accountsTransactions = AccountsTransactions(accounts, recipientRnd, timestampRnd);
            var loansTransactions = LoansTransactions(loansAndPayments.Select(lp => lp.loan).ToArray(), loansAndPayments.Select(lp => lp.payment).ToArray());
            var cardsTransactions = CardsTransactions(cards, recipientRnd, timestampRnd);
            var paymentsTransactions = PaymentsTransactions(activePayments);*/
            /*
            var allTransactions = accountsTransactions
                .Concat(loansTransactions)
                .Concat(cardsTransactions)
                .Concat(paymentsTransactions)
                .ToArray();*/

            var setupall = new SetupAll
            {
                AccountsSetup = new AccountsSetup { Accounts = accounts },
                CardsSetup = new CardsSetup { Cards = cards },
                LoansSetup = new LoansSetup { Loans = loansAndPayments.Select(x => x.loan).ToArray() },
                PaymentsSetup = new PaymentsSetup { Payments = loansAndPayments.Select(x => x.payment).ToArray() },
                UsersSetup = new UsersSetup { Users = users },
                TransactionsSetup = new TransactionsSetup { Transactions = new TransactionDTO[0] },
            };

            return setupall;
        }

        static AccountDTO[] Accounts(UserDTO[] users)
        {
            var countRnd = new RndBuilder<int>()
              .DistributionValues(new[] { 1, 2, 3, 4 })
              .DistributionProbabilities(new[] { 45, 30, 15, 10 })
              .Build();

            var amountRnd = new RndBuilder<float>(new CurrencyRnd())
                .Min(10)
                .Max(30000)
                .Build();

            return ValuesGenerator.CreateAccounts(users, countRnd, amountRnd).ToArray();
        }

        static CardDTO[] Cards(AccountDTO[] accounts)
        {
            var countRnd = new RndBuilder<int>()
              .DistributionValues(new[] { 0, 1, 2 })
              .DistributionProbabilities(new[] { 50, 40, 10 })
              .Build();

            return ValuesGenerator.CreateCards(accounts, countRnd).ToArray();
        }

        static PaymentDTO[] ActivePayments(AccountDTO[] accounts, IRnd<string> recipientRnd, IRnd<DateTime> timestampRnd)
        {
            var countRnd = new RndBuilder<int>()
              .DistributionValues(new[] { 0, 1, 2 })
              .DistributionProbabilities(new[] { 50, 40, 10 })
              .Build();

            var amountRnd = new RndBuilder<float>(new CurrencyRnd()).Min(20).Max(20000)
                .DistributionValues(new float[] { 500, 1000, 3000, 8000, 15000 })
                .DistributionProbabilities(new[] { 30, 20, 30, 10, 7, 3 })
                .Build(); //dystrybuanta
            var startDateRnd = timestampRnd;

            var intervalRnd = new RndBuilder<TimeSpan>()
              .DistributionValues(new[] { TimeSpan.FromDays(30), TimeSpan.FromDays(90), TimeSpan.FromDays(180), TimeSpan.FromDays(365), })
              .DistributionProbabilities(new[] { 75, 10, 10, 5 })
              .Build();

            return ValuesGenerator.CreatePayments(accounts, PaymentStatus.ACTIVE, recipientRnd, countRnd, amountRnd, startDateRnd, intervalRnd).ToArray();
        }

        static (LoanDTO loan, PaymentDTO payment)[] ActiveLoans(AccountDTO[] accounts, IRnd<string> recipientRnd)
        {
            var countRnd = new RndBuilder<int>()
              .DistributionValues(new[] { 0, 1, 2 })
              .DistributionProbabilities(new[] { 50, 40, 10 })
              .Build();

            var totalRnd = new RndBuilder<float>(new CurrencyRnd(false)).Min(100).Max(1000000)
                .DistributionValues(new float[] { 3000, 10000, 30000, 50000, 100000, 300000, 500000 })
                .DistributionProbabilities(new[] { 30, 10, 10, 10, 8, 20, 10, 2 })
                .Build(); //dystrybuanta

            var instalmentsRnd = new LoanInstalmentsRnd(); //todo w zaleznosci od totalRnd
            var paidInstalmentsRnd = (Rnd<int>)new RndBuilder<int>().Min(0).Build();

            var intervalRnd = new RndBuilder<TimeSpan>()
              .DistributionValues(new[] { TimeSpan.FromDays(14), TimeSpan.FromDays(30), TimeSpan.FromDays(60) })
              .DistributionProbabilities(new[] { 20, 70, 10 })
              .Build();
            return ValuesGenerator.CreateLoans(accounts, totalRnd, instalmentsRnd, paidInstalmentsRnd, intervalRnd, recipientRnd).ToArray();
        }

        static TransactionDTO[] AccountsTransactions(AccountDTO[] accounts, IRnd<string> recipientRnd, IRnd<DateTime> timestampRnd)
        {
            var countRnd = new RndBuilder<int>().Min(0).Max(100).Build(); //przykladowo   init: 0-3000
            var amountRnd = new RndBuilder<float>(new CurrencyRnd()).Min(10).Max(70000)
                .DistributionValues(new float[] { 200, 500, 1000, 3000, 5000, 10000, 30000, 50000 })
                .DistributionProbabilities(new[] { 30, 20, 10, 15, 10, 5, 5, 3, 2 })
                .Build();//dystrybuanta - max bardzo rzadko, min bardzo często
            var titleRnd = new RndBuilder<string>(new TitleRnd()).Build();

            return ValuesGenerator.CreateTransactions(accounts, recipientRnd, countRnd, timestampRnd, amountRnd, titleRnd).ToArray();
        }

        static TransactionDTO[] LoansTransactions(LoanDTO[] loans, PaymentDTO[] payments)
        {
            var paymentsDict = payments.ToDictionary(k => k.Id, k => k);
            return ValuesGenerator.CreateTransactions(loans, paymentsDict).ToArray();
        }

        static TransactionDTO[] PaymentsTransactions(PaymentDTO[] payments)
        {
            return ValuesGenerator.CreateTransactions(payments).ToArray();
        }

        static TransactionDTO[] CardsTransactions(CardDTO[] cards, IRnd<string> recipientRnd, IRnd<DateTime> timestampRnd)
        {
            var countRnd = new RndBuilder<int>().Min(50).Max(100).Build(); //przykladowo  50-20000
            var amountRnd = new RndBuilder<float>(new CurrencyRnd()).Min(5).Max(20000)
                .DistributionValues(new float[] { 20, 100, 300, 500, 1000, 2000, 5000, 10000 })
                .DistributionProbabilities(new[] { 35, 30, 20, 5, 5, 2, 1, 1, 1 })
                .Build(); //dystrybuanta - max bardzo rzadko, min bardzo często
            return ValuesGenerator.CreateTransactions(cards, recipientRnd, countRnd, timestampRnd, amountRnd).ToArray();
        }
    }
}

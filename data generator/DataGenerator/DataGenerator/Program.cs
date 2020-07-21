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
            var users = Generator.CreateUsers(1000).ToArray();
            var accounts = Accounts(users);
            var recipientRnd = new RndBuilder<string>().DistributionValues(accounts.Select(a => a.Id)).Build();
            var timestampRnd = new RndBuilder<DateTime>().Min(new DateTime(2015, 1, 1)).Max(new DateTime(2020, 8, 1)).Build();

            var cards = Cards(accounts);
            var activePayments = ActivePayments(accounts, recipientRnd, timestampRnd);
            var loansAndPayments = ActiveLoans(accounts, recipientRnd);

            var lnsct = loansAndPayments.Count(l => l.payment.StartTimestamp < new DateTime(1950, 1, 1));

            var accountsTransactions = AccountsTransactions(accounts, recipientRnd, timestampRnd);
            var loansTransactions = LoansTransactions(loansAndPayments.Select(lp => lp.loan).ToArray(), loansAndPayments.Select(lp => lp.payment).ToArray());
            var cardsTransactions = CardsTransactions(cards, recipientRnd, timestampRnd);
            var paymentsTransactions = PaymentsTransactions(activePayments);

            var allTransactions = accountsTransactions
                .Concat(loansTransactions)
                .Concat(cardsTransactions)
                .Concat(paymentsTransactions)
                .ToArray();

            var setupall = new SetupAll
            {
                AccountsSetup = new AccountsSetup { Accounts = accounts },
                CardsSetup = new CardsSetup { Cards = cards },
                LoansSetup = new LoansSetup { Loans = loansAndPayments.Select(x => x.loan).ToArray() },
                PaymentsSetup = new PaymentsSetup { Payments = loansAndPayments.Select(x => x.payment).ToArray() },
                UsersSetup = new UsersSetup { Users = users },
                TransactionsSetup = new TransactionsSetup { Transactions = allTransactions },
            };

            var json = JSON.Serialize(setupall);
            File.WriteAllText("setup.json", json);
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

            return Generator.CreateAccounts(users, countRnd, amountRnd).ToArray();
        }

        static CardDTO[] Cards(AccountDTO[] accounts)
        {
            var countRnd = new RndBuilder<int>()
              .DistributionValues(new[] { 0, 1, 2 })
              .DistributionProbabilities(new[] { 50, 40, 10 })
              .Build();

            return Generator.CreateCards(accounts, countRnd).ToArray();
        }

        static PaymentDTO[] ActivePayments(AccountDTO[] accounts, IRnd<string> recipientRnd, IRnd<DateTime> timestampRnd)
        {
            var countRnd = new RndBuilder<int>()
              .DistributionValues(new[] { 0, 1, 2 })
              .DistributionProbabilities(new[] { 50, 40, 10 })
              .Build();

            var amountRnd = new RndBuilder<float>(new CurrencyRnd()).Min(20).Max(10000).Build(); //dystrybuanta / stan konta
            var startDateRnd = timestampRnd;

            var intervalRnd = new RndBuilder<TimeSpan>()
              .DistributionValues(new[] { TimeSpan.FromDays(1), TimeSpan.FromDays(7), TimeSpan.FromDays(30), TimeSpan.FromDays(90), TimeSpan.FromDays(180), TimeSpan.FromDays(365), })
              .DistributionProbabilities(new[] { 5, 10, 60, 10, 10, 5 })
              .Build();

            return Generator.CreatePayments(accounts, PaymentStatus.ACTIVE, recipientRnd, countRnd, amountRnd, startDateRnd, intervalRnd).ToArray();
        }

        static (LoanDTO loan, PaymentDTO payment)[] ActiveLoans(AccountDTO[] accounts, IRnd<string> recipientRnd)
        {
            var countRnd = new RndBuilder<int>()
              .DistributionValues(new[] { 0, 1, 2 })
              .DistributionProbabilities(new[] { 50, 40, 10 })
              .Build();

            var totalRnd = new RndBuilder<float>(new CurrencyRnd(false)).Min(100).Max(1000000).Build(); //dystrybuanta

            var instalmentsRnd = new RndBuilder<int>().DistributionValues(new[] { 3, 6, 12, 24, 36, 48, 60, 120, 180, 240, 360 }).Build(); //todo w zaleznosci od totalRnd
            var paidInstalmentsRnd = (Rnd<int>)new RndBuilder<int>().Min(0).Build(); //todo w zaleznosci od instalmentsRnd

            var intervalRnd = new RndBuilder<TimeSpan>()
              .DistributionValues(new[] { TimeSpan.FromDays(30), TimeSpan.FromDays(60), TimeSpan.FromDays(180), TimeSpan.FromDays(90), TimeSpan.FromDays(180), TimeSpan.FromDays(365), })
              .DistributionProbabilities(new[] { 85, 7, 3, 2, 2, 1 })
              .Build(); ;
            return Generator.CreateLoans(accounts, totalRnd, instalmentsRnd, paidInstalmentsRnd, intervalRnd, recipientRnd).ToArray();
        }

        static TransactionDTO[] AccountsTransactions(AccountDTO[] accounts, IRnd<string> recipientRnd, IRnd<DateTime> timestampRnd)
        {
            var countRnd = new RndBuilder<int>().Min(0).Max(100).Build(); //przykladowo   init: 0-3000
            var amountRnd = new RndBuilder<float>(new CurrencyRnd()).Min(10).Max(70000).Build();//dystrybuanta - max bardzo rzadko, min bardzo często
            var titleRnd = new RndBuilder<string>(new TitleRnd()).Build();

            return Generator.CreateTransactions(accounts, recipientRnd, countRnd, timestampRnd, amountRnd, titleRnd).ToArray();
        }

        static TransactionDTO[] LoansTransactions(LoanDTO[] loans, PaymentDTO[] payments)
        {
            var paymentsDict = payments.ToDictionary(k => k.Id, k => k);
            return Generator.CreateTransactions(loans, paymentsDict).ToArray();
        }

        static TransactionDTO[] PaymentsTransactions(PaymentDTO[] payments)
        {
            return Generator.CreateTransactions(payments).ToArray();
        }

        static TransactionDTO[] CardsTransactions(CardDTO[] cards, IRnd<string> recipientRnd, IRnd<DateTime> timestampRnd)
        {
            var countRnd = new RndBuilder<int>().Min(50).Max(100).Build(); //przykladowo  50-20000
            var amountRnd = new RndBuilder<float>(new CurrencyRnd()).Min(2).Max(15000).Build(); //dystrybuanta - max bardzo rzadko, min bardzo często
            return Generator.CreateTransactions(cards, recipientRnd, countRnd, timestampRnd, amountRnd).ToArray();
        }
    }
}

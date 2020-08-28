using DataGenerator.DTO;
using DataGenerator.Rnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGenerator.Generators
{
    public static class ReportDataGenerator
    {
        //cards loans payments transactions
        public static SetupAll GenerateOverallReportData(int usersCount, int cardsTC, int loansTC, int paymentsTC, int accountsTC, DateTime minDate, DateTime maxDate)
        {
            var users = ValuesGenerator.CreateUsers(usersCount).ToArray();
            var accounts = Accounts(users);
            var recipientRnd = new RndBuilder<string>().DistributionValues(accounts.Select(a => a.Id)).Build();
            var timestampRnd = new RndBuilder<DateTime>().Min(minDate).Max(maxDate).Build();

            var cards = Cards(accounts);
            var activePayments = ActivePayments(accounts, recipientRnd, timestampRnd);
            var loansAndPayments = ActiveLoans(accounts, recipientRnd);

            var accountsTransactions = AccountsTransactions(accountsTC, accounts, recipientRnd, timestampRnd);
            var loansTransactions = LoansTransactions(loansTC, loansAndPayments.Select(lp => lp.loan).ToArray(), loansAndPayments.Select(lp => lp.payment).ToArray(), timestampRnd);
            var cardsTransactions = CardsTransactions(cardsTC, cards, recipientRnd, timestampRnd);
            var paymentsTransactions = PaymentsTransactions(paymentsTC, activePayments, timestampRnd);

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
                PaymentsSetup = new PaymentsSetup { Payments = loansAndPayments.Select(x => x.payment).Concat(activePayments).ToArray() },
                UsersSetup = new UsersSetup { Users = users },
                TransactionsSetup = new TransactionsSetup { Transactions = allTransactions },
            };

            return setupall;
        }

        static AccountDTO[] Accounts(UserDTO[] users)
        {
            var countRnd = new RndBuilder<int>()
              .DistributionValues(new[] { 1, 2 })
              .DistributionProbabilities(new[] { 80, 20 })
              .Build();

            var amountRnd = new RndBuilder<float>(new CurrencyRnd())
                .Min(500)
                .Max(50000)
                .DistributionValues(new float[] { 3000, 5000, 10000, 15000, 20000, 35000 })
                .DistributionProbabilities(new int[] { 20, 25, 20, 15, 10, 5, 5 })
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
            var titleRnd = (TitleRnd)new RndBuilder<string>(new TitleRnd()).Build();
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

            return ValuesGenerator.CreatePayments(accounts, PaymentStatus.ACTIVE, recipientRnd, countRnd, amountRnd, startDateRnd, () => DateTime.UtcNow, intervalRnd, titleRnd).ToArray();
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
            return ValuesGenerator.CreateLoans(accounts, countRnd, totalRnd, instalmentsRnd, paidInstalmentsRnd, intervalRnd, () => DateTime.UtcNow, recipientRnd).ToArray();
        }

        static IEnumerable<TransactionDTO> AccountsTransactions(int accountsTC, AccountDTO[] accounts, IRnd<string> recipientRnd, IRnd<DateTime> timestampRnd)
        {
            var rand = new Random();
            var countRnd = new RndBuilder<int>().Min(0).Max(100).Build(); //przykladowo   init: 0-3000
            var amountRnd = new RndBuilder<float>(new CurrencyRnd()).Min(10).Max(70000)
                .DistributionValues(new float[] { 200, 500, 1000, 3000, 5000, 10000, 30000, 50000 })
                .DistributionProbabilities(new[] { 30, 20, 10, 15, 10, 5, 5, 3, 2 })
                .Build();//dystrybuanta - max bardzo rzadko, min bardzo często
            var titleRnd = new RndBuilder<string>(new TitleRnd()).Build();

            for (int i = 0; i < accountsTC; i++)
            {
                var sender = recipientRnd.Next();
                var recipient = recipientRnd.Next(sender);
                yield return new TransactionDTO
                {
                    Id = Guid.NewGuid().ToString(),
                    Amount = amountRnd.Next(),
                    Sender = sender,
                    Timestamp = timestampRnd.Next(),
                    Recipient = recipient,
                    Title = titleRnd.Next()
                };
            }
        }

        static IEnumerable<TransactionDTO> LoansTransactions(int loansTC, LoanDTO[] loans, PaymentDTO[] payments, IRnd<DateTime> timestampRnd)
        {
            var rand = new Random();
            var paymentsDict = payments.ToDictionary(k => k.Id, k => k);
            for (int i = 0; i < loansTC; i++)
            {
                var loan = loans[rand.Next(0, payments.Length)];
                var payment = paymentsDict[loan.PaymentId];
                var timestamp = timestampRnd.Next();
                yield return new TransactionDTO
                {
                    Id = Guid.NewGuid().ToString(),
                    Amount = payment.Amount,
                    PaymentId = payment.Id,
                    Sender = payment.AccountId,
                    Recipient = payment.Recipient,
                    Timestamp = timestamp,
                    Title = $"{i + 1} of {loan.Instalments} instalments. Payment {"nejm"}({payment.Id})"
                };
            }
        }

        static IEnumerable<TransactionDTO> PaymentsTransactions(int paymentsTC, PaymentDTO[] payments, IRnd<DateTime> timestampRnd)
        {
            var rand = new Random();
            for (int i = 0; i < paymentsTC; i++)
            {
                var payment = payments[rand.Next(0, payments.Length)];
                var timestamp = timestampRnd.Next();
                yield return new TransactionDTO
                {
                    Id = Guid.NewGuid().ToString(),
                    Amount = payment.Amount,
                    PaymentId = payment.Id,
                    Sender = payment.AccountId,
                    Recipient = payment.Recipient,
                    Timestamp = timestamp,
                    Title = $"Transaction of payment {"nejm"}({payment.Id})"
                };
            }
        }

        static IEnumerable<TransactionDTO> CardsTransactions(int cardsTC, CardDTO[] cards, IRnd<string> recipientRnd, IRnd<DateTime> timestampRnd)
        {
            var rand = new Random();
            var countRnd = new RndBuilder<int>().Min(50).Max(100).Build(); //przykladowo  50-20000
            var amountRnd = new RndBuilder<float>(new CurrencyRnd()).Min(5).Max(20000)
                .DistributionValues(new float[] { 20, 100, 300, 500, 1000, 2000, 5000, 10000 })
                .DistributionProbabilities(new[] { 35, 30, 20, 5, 5, 2, 1, 1, 1 })
                .Build(); //dystrybuanta - max bardzo rzadko, min bardzo często

            for (int i = 0; i < cardsTC; i++)
            {
                var card = cards[rand.Next(0, cards.Length)];
                var timestamp = timestampRnd.Next();
                var amount = amountRnd.Next();
                yield return new TransactionDTO
                {
                    Id = Guid.NewGuid().ToString(),
                    Amount = amount,
                    CardId = card.Id,
                    Sender = card.AccountId,
                    Timestamp = timestamp,
                    Recipient = recipientRnd.Next(card.AccountId),
                    Title = $"{timestamp} card usage for a transfer worth {amount} EUR."
                };
            }
        }
    }
}

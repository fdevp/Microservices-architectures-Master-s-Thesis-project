using DataGenerator.DTO;
using DataGenerator.Rnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGenerator.Generators
{
    public static class AutomatDataGenerator
    {
        public static SetupAll Generate(int usersCount, DateTime minDate, DateTime maxDate)
        {
            var users = ValuesGenerator.CreateUsers(usersCount).ToArray();
            var accounts = Accounts(users);
            var recipientRnd = new RndBuilder<string>().DistributionValues(accounts.Select(a => a.Id)).Build();
            var timestampRnd = new RndBuilder<DateTime>().Min(minDate).Max(maxDate).Build();

            var activePayments = ActivePayments(accounts, recipientRnd, timestampRnd);
            var loansAndPayments = ActiveLoans(accounts, recipientRnd);

            var totalPaymentsCount = activePayments.Count() + loansAndPayments.Count();
            var interval = (maxDate - minDate) / totalPaymentsCount;
            var date = minDate;

            var dates = new List<DateTime>();
            for (int i = 0; i < totalPaymentsCount; i++)
            {
                dates.Add(date);
                date += interval;
            }
            var shuffledDates = dates.OrderBy(d => Guid.NewGuid()).ToArray();

            for (int i = 0; i < activePayments.Length; i++)
            {
                var paymentDate = shuffledDates[i];
                activePayments[i].LatestProcessingTimestamp = paymentDate - activePayments[i].Interval;
                activePayments[i].StartTimestamp = paymentDate - 2 * activePayments[i].Interval;
            }

            for (int i = 0; i < loansAndPayments.Length; i++)
            {
                var pair = loansAndPayments[i];
                var paymentDate = shuffledDates[i + activePayments.Length];
                pair.payment.LatestProcessingTimestamp = paymentDate - pair.payment.Interval;
                pair.payment.StartTimestamp = paymentDate - ((pair.loan.PaidAmount / pair.loan.TotalAmount) * pair.loan.Instalments) * pair.payment.Interval - pair.payment.Interval;
            }

            var setupall = new SetupAll
            {
                AccountsSetup = new AccountsSetup { Accounts = accounts },
                LoansSetup = new LoansSetup { Loans = loansAndPayments.Select(x => x.loan).ToArray() },
                PaymentsSetup = new PaymentsSetup { Payments = loansAndPayments.Select(x => x.payment).Concat(activePayments).ToArray() },
                UsersSetup = new UsersSetup { Users = users },

                CardsSetup = new CardsSetup { Cards = new CardDTO[0] },
                TransactionsSetup = new TransactionsSetup { Transactions = new TransactionDTO[0] },
            };

            return setupall;
        }

        static PaymentDTO[] ActivePayments(AccountDTO[] accounts, IRnd<string> recipientRnd, IRnd<DateTime> timestampRnd)
        {
            var titleRnd = (TitleRnd)new RndBuilder<string>(new TitleRnd()).Build();
            var countRnd = new RndBuilder<int>()
              .DistributionValues(new[] { 1, 3, 5, 10 })
              .DistributionProbabilities(new[] { 10, 40, 30, 20 })
              .Build();

            var amountRnd = new RndBuilder<float>(new CurrencyRnd()).Min(200).Max(3000)
                .DistributionValues(new float[] { 500, 900, 1500, 2000 })
                .DistributionProbabilities(new[] { 30, 30, 20, 10, 10 })
                .Build(); //dystrybuanta
            var startDateRnd = timestampRnd;

            var intervalRnd = new RndBuilder<TimeSpan>()
              .DistributionValues(new[] { TimeSpan.FromDays(7), TimeSpan.FromDays(14), TimeSpan.FromDays(21), TimeSpan.FromDays(28), })
              .DistributionProbabilities(new[] { 75, 10, 10, 5 })
              .Build();

            return ValuesGenerator.CreatePayments(accounts, PaymentStatus.ACTIVE, recipientRnd, countRnd, amountRnd, startDateRnd, () => DateTime.UtcNow, intervalRnd, titleRnd).ToArray();
        }

        static (LoanDTO loan, PaymentDTO payment)[] ActiveLoans(AccountDTO[] accounts, IRnd<string> recipientRnd)
        {
            var countRnd = new RndBuilder<int>()
              .DistributionValues(new[] { 1, 3, 5 })
              .DistributionProbabilities(new[] { 20, 50, 30 })
              .Build();

            var totalRnd = new RndBuilder<float>(new CurrencyRnd(false)).Min(100).Max(300000)
                .DistributionValues(new float[] { 3000, 10000, 30000, 50000, 100000 })
                .DistributionProbabilities(new[] { 5, 20, 30, 15, 15, 15 })
                .Build(); //dystrybuanta

            var instalmentsRnd = new LoanInstalmentsRnd(); //todo w zaleznosci od totalRnd
            var paidInstalmentsRnd = new CustomPaidInstalmentsRnd(2);

            var intervalRnd = new RndBuilder<TimeSpan>()
              .DistributionValues(new[] { TimeSpan.FromDays(7), TimeSpan.FromDays(14), TimeSpan.FromDays(21), TimeSpan.FromDays(28), })
              .DistributionProbabilities(new[] { 75, 10, 10, 5 })
              .Build();
            return ValuesGenerator.CreateLoans(accounts, countRnd, totalRnd, instalmentsRnd, paidInstalmentsRnd, intervalRnd, () => DateTime.UtcNow, recipientRnd).ToArray();
        }

        static AccountDTO[] Accounts(UserDTO[] users)
        {
            var countRnd = new RndBuilder<int>()
              .DistributionValues(new[] { 1, 2, 3, 4 })
              .DistributionProbabilities(new[] { 45, 30, 15, 10 })
              .Build();

            var amountRnd = new RndBuilder<float>(new CurrencyRnd())
                .Min(500)
                .Max(50000)
                .DistributionValues(new float[] { 3000, 5000, 10000, 15000, 20000, 35000 })
                .DistributionProbabilities(new int[] { 20, 25, 20, 15, 10, 5, 5 })
                .Build();

            return ValuesGenerator.CreateAccounts(users, countRnd, amountRnd).ToArray();
        }

        private class CustomPaidInstalmentsRnd : IntRnd
        {
            private readonly int maxInstalments;

            public CustomPaidInstalmentsRnd(int maxInstalments)
            {
                this.maxInstalments = maxInstalments;
                this.Min = 0;
            }

            public override int Next()
            {
                this.Max = maxInstalments;
                return base.Next();
            }
        }
    }
}

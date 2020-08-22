using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SharedClasses;

namespace ReportsBranchMicroservice
{
    public static class ReportGenerator
    {

        public static string AggregateOverall(OverallReportData data)
        {
            var withTimestamps = data.Transactions.Select(t => new TransactionWithTimestamp { Timestamp = t.Timestamp.ToDateTime(), Transaction = t });
            var periods = GroupByPeriods(data.Granularity, withTimestamps);

            var portions = new List<OverallReportPortion>();
            foreach (var period in periods)
            {
                foreach (var aggregation in data.Aggregations)
                {
                    var value = Aggregate(period, aggregation);
                    portions.Add(new OverallReportPortion { Period = period.Key, Value = value, Aggregation = aggregation });
                }
            }

            return ReportCsvSerializer.SerializerOverallReport(data, portions.ToArray());
        }

        public static string AggregateUserActivity(UserActivityRaportData data)
        {
            var withTimestamps = data.Transactions.Select(t => new TransactionWithTimestamp { Timestamp = t.Timestamp.ToDateTime(), Transaction = t });

            var accountsPortion = data.Accounts.SelectMany(a => AggregateUserAccountsTransactions(a, withTimestamps, data.Granularity)).ToArray();

            var cardsTransactions = withTimestamps.Where(t => t.Transaction.CardId != null);
            var cardsPortion = data.Cards.SelectMany(c => AggregateUserCardTransactions(c, cardsTransactions, data.Granularity)).ToArray();

            var paymentsTransactions = withTimestamps.Where(t => t.Transaction.PaymentId != null);
            var paymentsPortion = data.Payments.SelectMany(p => AggregateUserPaymentsTransactions(p, paymentsTransactions, data.Granularity)).ToArray();
            var loansPortion = data.Loans.SelectMany(l => AggregateUserLoansTransactions(l, paymentsTransactions, data.Granularity)).ToArray();

            return ReportCsvSerializer.SerializerUserActivityReport(data, accountsPortion, cardsPortion, paymentsPortion, loansPortion);
        }

        private static IEnumerable<IGrouping<string, TransactionWithTimestamp>> GroupByPeriods(Granularity granularity, IEnumerable<TransactionWithTimestamp> transactions)
        {
            switch (granularity)
            {
                case Granularity.Day:
                    return transactions.GroupBy(t => t.Timestamp.ToString("yyyy-MM-dd"));
                case Granularity.Week:
                    return transactions.GroupBy(t => $"{GetDate(t.Timestamp, 1)} do {GetDate(t.Timestamp, 7)}");
                case Granularity.Month:
                    return transactions.GroupBy(t => t.Timestamp.ToString("yyyy-MM"));
                case Granularity.Year:
                    return transactions.GroupBy(t => t.Timestamp.ToString("yyyy"));
                case Granularity.All:
                    return transactions.GroupBy(t => "All time");
                default:
                    throw new InvalidOperationException("Unknown granularity");
            }
        }

        private static string GetDate(DateTime date, int dayOfWeek) => date.AddDays(-(int)date.DayOfWeek + dayOfWeek).ToString("yyyy-MM-dd");

        private static float Aggregate(IGrouping<string, TransactionWithTimestamp> period, Aggregation aggregation)
        {
            switch (aggregation)
            {
                case Aggregation.Count:
                    return period.Count();
                case Aggregation.Avg:
                    return period.Average(t => t.Transaction.Amount);
                case Aggregation.Sum:
                    return period.Sum(p => p.Transaction.Amount);
                case Aggregation.Min:
                    return period.Min(t => t.Transaction.Amount);
                case Aggregation.Max:
                    return period.Max(t => t.Transaction.Amount);
                default:
                    throw new InvalidOperationException("Unknown aggregation.");
            }
        }

        private static IEnumerable<UserReportPortion> AggregateUserCardTransactions(Card card, IEnumerable<TransactionWithTimestamp> allTransactions, Granularity granularity)
        {
            var transactions = allTransactions.Where(t => t.Transaction.CardId == card.Id);
            var portions = GroupByPeriods(granularity, transactions);
            foreach (var portion in portions)
            {
                var debits = portion.Sum(p => (float?)p.Transaction.Amount) ?? 0;
                yield return new UserReportPortion { Period = portion.Key, Debits = debits, Element = card.Number };
            }
        }

        private static IEnumerable<UserReportPortion> AggregateUserAccountsTransactions(Account account, IEnumerable<TransactionWithTimestamp> allTransactions, Granularity granularity)
        {
            var transactions = allTransactions.Where(t => t.Transaction.Sender == account.Id || t.Transaction.Recipient == account.Id);
            var portions = GroupByPeriods(granularity, transactions);
            foreach (var portion in portions)
            {
                var incomes = portion.Where(p => p.Transaction.Recipient == account.Id).Sum(p => (float?)p.Transaction.Amount) ?? 0;
                var debits = portion.Where(p => p.Transaction.Sender == account.Id).Sum(p => (float?)p.Transaction.Amount) ?? 0;
                yield return new UserReportPortion { Period = portion.Key, Debits = debits, Incomes = incomes, Element = account.Number };
            }
        }

        private static IEnumerable<UserReportPortion> AggregateUserLoansTransactions(Loan loan, IEnumerable<TransactionWithTimestamp> allTransactions, Granularity granularity)
        {
            var transactions = allTransactions.Where(p => p.Transaction.PaymentId == loan.PaymentId);
            var portions = GroupByPeriods(granularity, transactions);
            foreach (var portion in portions)
            {
                var debits = portion.Sum(p => (float?)p.Transaction.Amount) ?? 0;
                yield return new UserReportPortion { Period = portion.Key, Debits = debits, Element = loan.Id };
            }
        }

        private static IEnumerable<UserReportPortion> AggregateUserPaymentsTransactions(Payment payment, IEnumerable<TransactionWithTimestamp> allTransactions, Granularity granularity)
        {
            var transactions = allTransactions.Where(p => p.Transaction.PaymentId == payment.Id);
            var portions = GroupByPeriods(granularity, transactions);
            foreach (var portion in portions)
            {
                var debits = portion.Sum(p => (float?)p.Transaction.Amount) ?? 0;
                yield return new UserReportPortion { Period = portion.Key, Debits = debits, Element = payment.Name };
            }
        }
    }
}
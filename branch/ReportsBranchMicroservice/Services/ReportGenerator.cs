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

        public static IEnumerable<OverallReportPortion> AggregateOverall(OverallReportData data)
        {
            var withTimestamps = data.Transactions.Select(t => new TransactionWithTimestamp { Timestamp = new DateTime(t.Timestamp), Transaction = t });
            var periods = GroupByPeriods(data.Granularity, withTimestamps);
            var ordered = periods.OrderBy(p => p.Key);
            foreach (var period in ordered)
            {
                foreach (var aggregation in data.Aggregations)
                {
                    var value = Aggregate(period, aggregation);
                    yield return new OverallReportPortion { Period = period.Key, Value = value, Aggregation = aggregation };
                }
            }
        }

        public static AggregateUserActivityResponse AggregateUserActivity(UserActivityRaportData data)
        {
            var withTimestamps = data.Transactions.Select(t => new TransactionWithTimestamp { Timestamp = new DateTime(t.Timestamp), Transaction = t });
            var portions = GroupByPeriods(data.Granularity, withTimestamps);
            var ordered = portions.OrderBy(p => p.Key);

            var aggregated = new AggregateUserActivityResponse();
            foreach (var portion in ordered)
            {
                aggregated.CardsPortions.AddRange(data.Cards.Select(card =>
                {
                    var debits = portion.Where(p => p.Transaction.CardId == card.Id).Sum(p => (float?)p.Transaction.Amount) ?? 0;
                    return new UserReportPortion { Period = portion.Key, Debits = debits, Element = card.Number };
                }));

                aggregated.PaymentsPortions.AddRange(data.Payments.Select(payment =>
                {
                    var debits = portion.Where(p => p.Transaction.PaymentId == payment.Id).Sum(p => (float?)p.Transaction.Amount) ?? 0;
                    return new UserReportPortion { Period = portion.Key, Debits = debits, Element = payment.Id };
                }));

                aggregated.AccountsPortions.AddRange(data.Accounts.Select(account =>
                {
                    var incomes = portion.Where(p => p.Transaction.Recipient == account.Id).Sum(p => (float?)p.Transaction.Amount) ?? 0;
                    var debits = portion.Where(p => p.Transaction.Sender == account.Id).Sum(p => (float?)p.Transaction.Amount) ?? 0;
                    return new UserReportPortion { Period = portion.Key, Debits = debits, Incomes = incomes, Element = account.Number };
                }));

                aggregated.LoansPortions.AddRange(data.Loans.Select(loan =>
                {
                    var debits = portion.Where(p => p.Transaction.PaymentId == loan.PaymentId).Sum(p => (float?)p.Transaction.Amount) ?? 0;
                    return new UserReportPortion { Period = portion.Key, Debits = debits, Element = loan.Id };
                }));
            }

            return aggregated;
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
    }
}
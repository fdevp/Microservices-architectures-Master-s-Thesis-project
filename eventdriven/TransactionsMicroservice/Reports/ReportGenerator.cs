using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SharedClasses;
using SharedClasses.Events.Reports;
using SharedClasses.Models;

namespace TransactionsMicroservice.Reports
{
    public static class ReportGenerator
    {

        public static IEnumerable<OverallReportPortion> AggregateOverall(OverallReportData data)
        {
            var periods = GroupByPeriods(data.Granularity, data.Transactions);
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

        public static AggregatedUserActivityReportEvent AggregateUserActivity(UserActivityRaportData data)
        {
            var portions = GroupByPeriods(data.Granularity, data.Transactions);
            var ordered = portions.OrderBy(p => p.Key);

            var cards = new List<UserReportPortion>();
            var payments = new List<UserReportPortion>();
            var accounts = new List<UserReportPortion>();
            var loans = new List<UserReportPortion>();

            foreach (var portion in ordered)
            {
                cards.AddRange(data.Cards.Select(card =>
                {
                    var debits = portion.Where(p => p.CardId == card.Id).Sum(p => (float?)p.Amount) ?? 0;
                    return new UserReportPortion { Period = portion.Key, Debits = debits, Element = card.Number };
                }));

                payments.AddRange(data.Payments.Select(payment =>
                {
                    var debits = portion.Where(p => p.PaymentId == payment.Id).Sum(p => (float?)p.Amount) ?? 0;
                    return new UserReportPortion { Period = portion.Key, Debits = debits, Element = payment.Id };
                }));

                accounts.AddRange(data.Accounts.Select(account =>
                {
                    var incomes = portion.Where(p => p.Recipient == account.Id).Sum(p => (float?)p.Amount) ?? 0;
                    var debits = portion.Where(p => p.Sender == account.Id).Sum(p => (float?)p.Amount) ?? 0;
                    return new UserReportPortion { Period = portion.Key, Debits = debits, Incomes = incomes, Element = account.Number };
                }));

                loans.AddRange(data.Loans.Select(loan =>
                {
                    var debits = portion.Where(p => p.PaymentId == loan.PaymentId).Sum(p => (float?)p.Amount) ?? 0;
                    return new UserReportPortion { Period = portion.Key, Debits = debits, Element = loan.Id };
                }));
            }

            return new AggregatedUserActivityReportEvent
            {
                AccountsPortions = accounts.ToArray(),
                CardsPortions = cards.ToArray(),
                LoansPortions = loans.ToArray(),
                PaymentsPortions = payments.ToArray()
            };
        }

        private static string GetDate(DateTime date, int dayOfWeek) => date.AddDays(-(int)date.DayOfWeek + dayOfWeek).ToString("yyyy-MM-dd");

        private static float Aggregate(IGrouping<string, Transaction> period, Aggregation aggregation)
        {
            switch (aggregation)
            {
                case Aggregation.Count:
                    return period.Count();
                case Aggregation.Avg:
                    return period.Average(t => t.Amount);
                case Aggregation.Sum:
                    return period.Sum(p => p.Amount);
                case Aggregation.Min:
                    return period.Min(t => t.Amount);
                case Aggregation.Max:
                    return period.Max(t => t.Amount);
                default:
                    throw new InvalidOperationException("Unknown aggregation.");
            }
        }

        private static IEnumerable<IGrouping<string, Transaction>> GroupByPeriods(ReportGranularity granularity, IEnumerable<Transaction> transactions)
        {
            switch (granularity)
            {
                case ReportGranularity.Day:
                    return transactions.GroupBy(t => t.Timestamp.ToString("yyyy-MM-dd"));
                case ReportGranularity.Week:
                    return transactions.GroupBy(t => $"{GetDate(t.Timestamp, 1)} do {GetDate(t.Timestamp, 7)}");
                case ReportGranularity.Month:
                    return transactions.GroupBy(t => t.Timestamp.ToString("yyyy-MM"));
                case ReportGranularity.Year:
                    return transactions.GroupBy(t => t.Timestamp.ToString("yyyy"));
                case ReportGranularity.All:
                    return transactions.GroupBy(t => "All time");
                default:
                    throw new InvalidOperationException("Unknown granularity");
            }
        }
    }
}
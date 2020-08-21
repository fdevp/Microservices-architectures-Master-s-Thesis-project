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
            foreach (var period in periods)
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
            var accounts = data.Accounts.SelectMany(a => AggregateUserAccountsTransactions(a, data.Transactions, data.Granularity));
            
            var cardsTransactions = data.Transactions.Where(t => t.CardId != null);
            var cards = data.Cards.SelectMany(c => AggregateUserCardTransactions(c, cardsTransactions, data.Granularity));

            var paymentsTransactions = data.Transactions.Where(t => t.PaymentId != null);
            var payments = data.Payments.SelectMany(p => AggregateUserPaymentsTransactions(p, paymentsTransactions, data.Granularity));
            var loans = data.Loans.SelectMany(l => AggregateUserLoansTransactions(l, paymentsTransactions, data.Granularity));

            return new AggregatedUserActivityReportEvent
            {
                AccountsPortions = accounts.ToArray(),
                CardsPortions = cards.ToArray(),
                LoansPortions = loans.ToArray(),
                PaymentsPortions = payments.ToArray()
            };
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

        private static string GetDate(DateTime date, int dayOfWeek) => date.AddDays(-(int)date.DayOfWeek + dayOfWeek).ToString("yyyy-MM-dd");

        private static IEnumerable<UserReportPortion> AggregateUserCardTransactions(Card card, IEnumerable<Transaction> allTransactions, ReportGranularity granularity)
        {
            var transactions = allTransactions.Where(t => t.CardId == card.Id);
            var portions = GroupByPeriods(granularity, transactions);
            foreach (var portion in portions)
            {
                var debits = portion.Sum(p => (float?)p.Amount) ?? 0;
                yield return new UserReportPortion { Period = portion.Key, Debits = debits, Element = card.Number };
            }
        }

        private static IEnumerable<UserReportPortion> AggregateUserAccountsTransactions(Account account, IEnumerable<Transaction> allTransactions, ReportGranularity granularity)
        {
            var transactions = allTransactions.Where(t => t.Sender == account.Id || t.Recipient == account.Id);
            var portions = GroupByPeriods(granularity, transactions);
            foreach (var portion in portions)
            {
                var incomes = portion.Where(p => p.Recipient == account.Id).Sum(p => (float?)p.Amount) ?? 0;
                var debits = portion.Where(p => p.Sender == account.Id).Sum(p => (float?)p.Amount) ?? 0;
                yield return new UserReportPortion { Period = portion.Key, Debits = debits, Incomes = incomes, Element = account.Number };
            }
        }

        private static IEnumerable<UserReportPortion> AggregateUserLoansTransactions(Loan loan, IEnumerable<Transaction> allTransactions, ReportGranularity granularity)
        {
            var transactions = allTransactions.Where(p => p.PaymentId == loan.PaymentId);
            var portions = GroupByPeriods(granularity, transactions);
            foreach (var portion in portions)
            {
                var debits = portion.Sum(p => (float?)p.Amount) ?? 0;
                yield return new UserReportPortion { Period = portion.Key, Debits = debits, Element = loan.Id };
            }
        }

        private static IEnumerable<UserReportPortion> AggregateUserPaymentsTransactions(Payment payment, IEnumerable<Transaction> allTransactions, ReportGranularity granularity)
        {
            var transactions = allTransactions.Where(p => p.PaymentId == payment.Id);
            var portions = GroupByPeriods(granularity, transactions);
            foreach (var portion in portions)
            {
                var debits = portion.Sum(p => (float?)p.Amount) ?? 0;
                yield return new UserReportPortion { Period = portion.Key, Debits = debits, Element = payment.Id };
            }
        }
    }
}
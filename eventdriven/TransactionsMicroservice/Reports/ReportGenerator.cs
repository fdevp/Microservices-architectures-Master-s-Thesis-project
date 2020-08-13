using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SharedClasses;
using SharedClasses.Models;

namespace TransactionsMicroservice.Reports
{
    public static class ReportGenerator
    {
        public static string CreateOverallCsvReport(OverallReportData data)
        {
            var periods = GroupByPeriods(data.Granularity, data.Transactions);
            var ordered = periods.OrderBy(p => p.Key);

            var sb = new StringBuilder();
            sb.AppendLine($"Raport całościowy dla; {data.Subject}");
            sb.AppendLine($"Zakres od; {data.From?.ToString() ?? "-"}");
            sb.AppendLine($"Zakres do; {data.To?.ToString() ?? "-"}");
            sb.AppendLine($"Granularność; {data.Granularity}");

            foreach (var period in ordered)
            {
                sb.AppendLine(period.Key);
                foreach (var aggregation in data.Aggregations)
                    sb.WriteAggragation(period, aggregation);
            }

            return sb.ToString();
        }

        public static string CreateUserActivityCsvReport(UserActivityRaportData data)
        {
            var periods = GroupByPeriods(data.Granularity, data.Transactions);
            var ordered = periods.OrderBy(p => p.Key);

            var sb = new StringBuilder();
            sb.AppendLine($"Raport aktywności użytkownika; {data.UserId}");
            sb.AppendLine($"Zakres od; {data.From?.ToString() ?? "-"}");
            sb.AppendLine($"Zakres do; {data.To?.ToString() ?? "-"}");
            sb.AppendLine($"Granularność; {data.Granularity}");

            sb.WriteAccountsData(ordered, data.Accounts);
            sb.WriteCardsData(ordered, data.Cards);
            sb.WritePaymentsData(ordered, data.Payments);
            sb.WriteLoansData(ordered, data.Loans);

            return sb.ToString();
        }
        private static IEnumerable<IGrouping<string, Transaction>> GroupByPeriods(Granularity granularity, IEnumerable<Transaction> transactions)
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

        private static string GetDate(DateTime date, int day) => date.AddDays(-(int)date.DayOfWeek + day).ToString("yyyy-MM-dd");

        private static void WriteAggragation(this StringBuilder sb, IGrouping<string, Transaction> period, Aggregation aggregation)
        {
            float? value = null;
            switch (aggregation)
            {
                case Aggregation.Count:
                    value = period.Count();
                    sb.AppendLine($";Ilość;{value ?? '-'}");
                    break;
                case Aggregation.Avg:
                    value = period.Average(t => (float?)t.Amount);
                    sb.AppendLine($";Średnia;{value ?? '-'}");
                    break;
                case Aggregation.Sum:
                    value = period.Sum(p => p.Amount);
                    sb.AppendLine($";Suma;{value ?? '-'}");
                    break;
                case Aggregation.Min:
                    value = period.Min(t => (float?)t.Amount);
                    sb.AppendLine($";Wartość minimalna;{value ?? '-'}");
                    break;
                case Aggregation.Max:
                    value = period.Max(t => (float?)t.Amount);
                    sb.AppendLine($";Wartość maksymalna;{value ?? '-'}");
                    break;
            }
        }

        private static void WriteLoansData(this StringBuilder sb, IEnumerable<IGrouping<string, Transaction>> periods, Loan[] loans)
        {
            foreach (var loan in loans)
            {
                sb.AppendLine($"Kredyt {loan.Id}:");
                foreach (var period in periods)
                {
                    var transactions = period.Where(t => t.PaymentId == loan.PaymentId).ToArray();
                    if (transactions.Any())
                    {
                        sb.AppendLine($";{period.Key}");
                        sb.AppendLine($";;spłacono;{transactions.Sum(t => t.Amount)}");
                    }
                }
            }
        }

        private static void WriteCardsData(this StringBuilder sb, IEnumerable<IGrouping<string, Transaction>> periods, Card[] cards)
        {
            foreach (var card in cards)
            {
                sb.AppendLine($"Karta {card.Number}:");
                foreach (var period in periods)
                {
                    var transactions = period.Where(t => t.CardId == card.Id).ToArray();
                    if (transactions.Any())
                    {
                        sb.AppendLine($";{period.Key}");
                        sb.AppendLine($";;obciążenia;{transactions.Sum(t => t.Amount)}");
                    }
                }
            }
        }

        private static void WritePaymentsData(this StringBuilder sb, IEnumerable<IGrouping<string, Transaction>> periods, Payment[] payments)
        {
            foreach (var payment in payments)
            {
                sb.AppendLine($"Płatność {payment.Id}:");
                foreach (var period in periods)
                {
                    var transactions = period.Where(t => t.PaymentId == payment.Id).ToArray();
                    if (transactions.Any())
                    {
                        sb.AppendLine($";{period.Key}");
                        sb.AppendLine($";;obciążenia;{transactions.Sum(t => t.Amount)}");
                    }
                }
            }
        }

        private static void WriteAccountsData(this StringBuilder sb, IEnumerable<IGrouping<string, Transaction>> periods, Account[] accounts)
        {
            foreach (var account in accounts)
            {
                sb.AppendLine($"Konto {account.Number}:");
                foreach (var period in periods)
                {
                    var incomes = period.Where(t => t.Recipient == account.Id).Sum(t => (float?)t.Amount) ?? 0;
                    var debits = period.Where(t => t.Sender == account.Id).Sum(t => (float?)t.Amount) ?? 0;
                    var balance = incomes - debits;

                    sb.AppendLine($";{period.Key}");
                    sb.AppendLine($";;przychody;{incomes}");
                    sb.AppendLine($";;obciążenia;{debits}");
                    sb.AppendLine($";;suma;{balance}");
                }
            }
        }
    }
}
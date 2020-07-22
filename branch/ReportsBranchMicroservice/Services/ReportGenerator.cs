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
        public static string CreateOverallCsvReport(OverallReportData data)
        {
            var withTimestamps = data.Transactions.Select(t => new TransactionWithTimestamp { Timestamp = new DateTime(t.Timestamp), Transaction = t });
            var periods = GroupByPeriods(data.Granularity, withTimestamps);
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
            var withTimestamps = data.Transactions.Select(t => new TransactionWithTimestamp { Timestamp = new DateTime(t.Timestamp), Transaction = t });
            var periods = GroupByPeriods(data.Granularity, withTimestamps);
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


    }
}
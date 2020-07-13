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
            var grouped = GroupByGranularity(data.Granularity, withTimestamps);

            var sb = new StringBuilder();
            sb.AppendLine($"Raport całościowy dla; {data.Subject}");
            sb.AppendLine($"Zakres od; {data.From}");
            sb.AppendLine($"Zakres do; {data.To}");
            sb.AppendLine($"Granularność; {data.Granularity}");

            foreach (var group in grouped)
            {
                foreach (var aggregation in data.Aggregations)
                    sb.WriteAggragation(group, aggregation);
            }

            return string.Empty;
        }

        public static string CreateUserActivityCsvReport(UserActivityRaportData data)
        {
            var withTimestamps = data.Transactions.Select(t => new TransactionWithTimestamp { Timestamp = new DateTime(t.Timestamp), Transaction = t });
            var grouped = GroupByGranularity(data.Granularity, withTimestamps);

            var sb = new StringBuilder();
            sb.AppendLine($"Raport aktywności użytkownika; {data.UserId}");
            sb.AppendLine($"Zakres od; {data.From}");
            sb.AppendLine($"Zakres do; {data.To}");
            sb.AppendLine($"Granularność; {data.Granularity}");

            sb.WriteAccountsData(grouped, data.Accounts);
            sb.WriteCardsData(grouped, data.Cards);
            sb.WritePaymentsData(grouped, data.Payments);
            sb.WriteLoansData(grouped, data.Loans);

            return sb.ToString();
        }
        private static IEnumerable<IGrouping<string, TransactionWithTimestamp>> GroupByGranularity(Granularity granularity, IEnumerable<TransactionWithTimestamp> transactions)
        {
            switch (granularity)
            {
                case Granularity.Day:
                    return transactions.GroupBy(t => t.Timestamp.ToString("dd/MM/yyyy"));
                case Granularity.Week:
                    return transactions.GroupBy(t => $"{GetDate(t.Timestamp, DayOfWeek.Monday)} - {GetDate(t.Timestamp, DayOfWeek.Sunday)}");
                case Granularity.Month:
                    return transactions.GroupBy(t => t.Timestamp.ToString("MM/yyyy"));
                case Granularity.Year:
                    return transactions.GroupBy(t => t.Timestamp.ToString("yyyy"));
                case Granularity.All:
                    return transactions.GroupBy(t => "All time");
                default:
                    throw new InvalidOperationException("Unknown granularity");
            }
        }

        private static string GetDate(DateTime date, DayOfWeek day) => date.AddDays(-(int)date.DayOfWeek + (int)day).ToString("dd/MM/yyyy");


    }
}
using System;
using System.Linq;
using System.Text;
using APIGateway.Models;

namespace APIGateway.Reports
{
    public static class ReportGenerator
    {
        public static string CreateOverallCsvReport(Models.ReportSubject subject, DateTime? from, DateTime? to, ReportGranularity granularity, OverallReportPortion[] portions)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Raport całościowy dla; {subject}");
            sb.AppendLine($"Zakres od; {from?.ToString() ?? "-"}");
            sb.AppendLine($"Zakres do; {to?.ToString() ?? "-"}");
            sb.AppendLine($"Granularność; {granularity}");

            var groupedPortions = portions.GroupBy(p => p.Period);
            var ordered = groupedPortions.OrderBy(p => p.Key);

            foreach (var portion in ordered)
            {
                sb.AppendLine(portion.Key);
                foreach (var aggregation in portion)
                    sb.WriteAggragation(portion.Key, aggregation.Value, aggregation.Aggregation);
            }

            return sb.ToString();
        }

        public static string CreateUserActivityCsvReport(string userId, DateTime? from, DateTime? to, ReportGranularity granularity, UserActivityReportPortions portions)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Raport aktywności użytkownika; {userId}");
            sb.AppendLine($"Zakres od; {from?.ToString() ?? "-"}");
            sb.AppendLine($"Zakres do; {to?.ToString() ?? "-"}");
            sb.AppendLine($"Granularność; {granularity}");

            sb.WriteAccountsData(portions.Accounts);
            sb.WriteCardsData(portions.Cards);
            sb.WritePaymentsData(portions.Payments);
            sb.WriteLoansData(portions.Loans);

            return sb.ToString();
        }
    }
}
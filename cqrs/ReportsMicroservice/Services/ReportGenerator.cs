using System;
using System.Linq;
using System.Text;

namespace ReportsMicroservice
{
    public static class ReportGenerator
    {
        public static string CreateOverallCsvReport(ReportSubject subject, DateTime? from, DateTime? to, Granularity granularity, OverallReportPortion[] portions)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Raport całościowy dla; {subject}");
            sb.AppendLine($"Zakres od; {from?.ToString() ?? "-"}");
            sb.AppendLine($"Zakres do; {to?.ToString() ?? "-"}");
            sb.AppendLine($"Granularność; {granularity}");

            var groupedPortions = portions.GroupBy(p => p.Period);

            foreach (var portion in groupedPortions)
            {
                foreach (var aggregation in portion)
                    sb.WriteAggragation(portion.Key, aggregation.Value, aggregation.Aggregation);
            }

            return sb.ToString();
        }

        public static string CreateUserActivityCsvReport(UserActivityRaportData data)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Raport aktywności użytkownika; {data.UserId}");
            sb.AppendLine($"Zakres od; {data.From?.ToString() ?? "-"}");
            sb.AppendLine($"Zakres do; {data.To?.ToString() ?? "-"}");
            sb.AppendLine($"Granularność; {data.Granularity}");

            sb.WriteAccountsData(data.Portions.Accounts);
            sb.WriteCardsData(data.Portions.Cards);
            sb.WritePaymentsData(data.Portions.Payments);
            sb.WriteLoansData(data.Portions.Loans);

            return sb.ToString();
        }
    }
}
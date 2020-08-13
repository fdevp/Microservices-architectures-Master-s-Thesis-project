using System;
using System.Linq;
using System.Text;
using APIGateway.Models;
using ReportsBranchMicroservice;

namespace APIGateway.Reports
{
    public static class ReportCsvSerializer
    {
        public static string SerializerOverallReport(Models.ReportSubject subject, DateTime? from, DateTime? to, ReportGranularity granularity, OverallReportPortion[] portions)
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

        public static string SerializerUserActivityReport(string userId, DateTime? from, DateTime? to, ReportGranularity granularity, AggregateUserActivityResponse portions)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Raport aktywności użytkownika; {userId}");
            sb.AppendLine($"Zakres od; {from?.ToString() ?? "-"}");
            sb.AppendLine($"Zakres do; {to?.ToString() ?? "-"}");
            sb.AppendLine($"Granularność; {granularity}");

            sb.WriteAccountsData(portions.AccountsPortions.ToArray());
            sb.WriteCardsData(portions.CardsPortions.ToArray());
            sb.WritePaymentsData(portions.PaymentsPortions.ToArray());
            sb.WriteLoansData(portions.LoansPortions.ToArray());

            return sb.ToString();
        }

        private static void WriteAggragation(this StringBuilder sb, string period, float value, Aggregation aggregation)
        {
            switch (aggregation)
            {
                case Aggregation.Count:
                    sb.AppendLine($";Ilość;{value}");
                    break;
                case Aggregation.Avg:
                    sb.AppendLine($";Średnia;{value}");
                    break;
                case Aggregation.Sum:
                    sb.AppendLine($";Sum;{value}");
                    break;
                case Aggregation.Min:
                    sb.AppendLine($";Wartość minimalna;{value}");
                    break;
                case Aggregation.Max:
                    sb.AppendLine($";Wartość maksymalna;{value}");
                    break;
            }
        }

        private static void WriteLoansData(this StringBuilder sb, UserReportPortion[] portions)
        {
            var groups = portions.GroupBy(p => p.Element);
            foreach (var group in groups)
            {
                sb.AppendLine($"Kredyt {group.Key}:");
                foreach (var period in group)
                {
                    sb.AppendLine($";{period.Period}");
                    sb.AppendLine($";;spłacono;{period.Debits}");
                }
            }
        }

        private static void WriteCardsData(this StringBuilder sb, UserReportPortion[] portions)
        {
            var groups = portions.GroupBy(p => p.Element);
            foreach (var group in groups)
            {
                sb.AppendLine($"Karta {group.Key}:");
                foreach (var period in group)
                {
                    sb.AppendLine($";{period.Period}");
                    sb.AppendLine($";;obciążenia;{period.Debits}");
                }
            }
        }

        private static void WritePaymentsData(this StringBuilder sb, UserReportPortion[] portions)
        {
            var groups = portions.GroupBy(p => p.Element);
            foreach (var group in groups)
            {
                sb.AppendLine($"Płatność {group.Key}:");
                foreach (var period in group)
                {
                    sb.AppendLine($";{period.Period}");
                    sb.AppendLine($";;obciążenia;{period.Debits}");
                }
            }
        }

        private static void WriteAccountsData(this StringBuilder sb, UserReportPortion[] portions)
        {

            var groups = portions.GroupBy(p => p.Element);
            foreach (var group in groups)
            {
                sb.AppendLine($"Konto {group.Key}:");
                foreach (var period in group)
                {
                    sb.AppendLine($";{period.Period}");
                    sb.AppendLine($";;przychody;{period.Incomes}");
                    sb.AppendLine($";;obciążenia;{period.Debits}");
                    sb.AppendLine($";;suma;{period.Incomes - period.Debits}");
                }
            }
        }
    }
}
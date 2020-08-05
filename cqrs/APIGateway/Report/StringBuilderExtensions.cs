using System.Linq;
using System.Text;

namespace APIGateway.Reports
{
    public static class StringBuilderExtensions
    {
        public static void WriteAggragation(this StringBuilder sb, string period, float value, Aggregation aggregation)
        {
            switch (aggregation)
            {
                case Aggregation.Count:
                    sb.AppendLine($";Ilość;{value}");
                    break;
                case Aggregation.Avg:
                    sb.AppendLine($";Średnia;{value}");
                    break;
                case Aggregation.Median:
                    sb.AppendLine($";Mediana;{value}");
                    break;
                case Aggregation.Min:
                    sb.AppendLine($";Wartość minimalna;{value}");
                    break;
                case Aggregation.Max:
                    sb.AppendLine($";Wartość maksymalna;{value}");
                    break;
            }
        }

        public static void WriteLoansData(this StringBuilder sb, UserReportPortion[] portions)
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

        public static void WriteCardsData(this StringBuilder sb, UserReportPortion[] portions)
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

        public static void WritePaymentsData(this StringBuilder sb, UserReportPortion[] portions)
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

        public static void WriteAccountsData(this StringBuilder sb, UserReportPortion[] portions)
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
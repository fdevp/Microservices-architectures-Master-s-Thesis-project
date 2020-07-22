using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharedClasses.Models;

namespace ReportsMicroservice
{
    public static class StringBuilderExtensions
    {
        public static void WriteAggragation(this StringBuilder sb, IGrouping<string, Transaction> period, Aggregation aggregation)
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
                case Aggregation.Median:
                    value = (float?)Median(period.Select(p => p.Amount));
                    sb.AppendLine($";Mediana;{value ?? '-'}");
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

        public static void WriteLoansData(this StringBuilder sb, IEnumerable<IGrouping<string, Transaction>> periods, Loan[] loans)
        {
            foreach (var loan in loans)
            {
                sb.AppendLine($"Kredyt {loan.Id}:");
                foreach (var period in periods)
                {
                    var debits = period.Where(t => t.PaymentId == loan.PaymentId).Sum(t => (float?)t.Amount);
                    sb.AppendLine($";{period.Key}");
                    sb.AppendLine($";;spłacono;{debits ?? '-'}");
                }
            }
        }

        public static void WriteCardsData(this StringBuilder sb, IEnumerable<IGrouping<string, Transaction>> periods, Card[] cards)
        {
            foreach (var card in cards)
            {
                sb.AppendLine($"Karta {card.Number}:");
                foreach (var period in periods)
                {
                    var debits = period.Where(t => t.CardId == card.Id).Sum(t => (float?)t.Amount);
                    sb.AppendLine($";{period.Key}");
                    sb.AppendLine($";;obciążenia;{debits ?? '-'}");
                }
            }
        }

        public static void WritePaymentsData(this StringBuilder sb, IEnumerable<IGrouping<string, Transaction>> periods, Payment[] payments)
        {
            foreach (var payment in payments)
            {
                sb.AppendLine($"Płatność {payment}:");
                foreach (var period in periods)
                {
                    var debits = period.Where(t => t.PaymentId == payment.Id).Sum(t => (float?)t.Amount);
                    sb.AppendLine($";{period.Key}");
                    sb.AppendLine($";;obciążenia;{debits ?? '-'}");
                }
            }
        }

        public static void WriteAccountsData(this StringBuilder sb, IEnumerable<IGrouping<string, Transaction>> periods, Account[] accounts)
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

        private static double? Median<T>(IEnumerable<T> source)
        {
            if (Nullable.GetUnderlyingType(typeof(T)) != null)
                source = source.Where(x => x != null);

            int count = source.Count();
            if (count == 0)
                return null;

            source = source.OrderBy(n => n);

            int midpoint = count / 2;
            if (count % 2 == 0)
                return (Convert.ToDouble(source.ElementAt(midpoint - 1)) + Convert.ToDouble(source.ElementAt(midpoint))) / 2.0;
            else
                return Convert.ToDouble(source.ElementAt(midpoint));
        }
    }
}
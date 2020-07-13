using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReportsBranchMicroservice
{
    public static class StringBuilderExtensions
    {
        public static void WriteAggragation(this StringBuilder sb, IGrouping<string, TransactionWithTimestamp> group, Aggregation aggregation)
        {
            float? value = null;
            switch (aggregation)
            {
                case Aggregation.Count:
                    value = group.Count();
                    sb.AppendLine($";Ilość;{value ?? '-'}");
                    break;
                case Aggregation.Avg:
                    value = group.Average(t => (float?)t.Transaction.Amount);
                    sb.AppendLine($";Średnia;{value ?? '-'}");
                    break;
                case Aggregation.Median:
                    value = (float?)Median(group);
                    sb.AppendLine($";Mediana;{value ?? '-'}");
                    break;
                case Aggregation.Min:
                    value = group.Min(t => (float?)t.Transaction.Amount);
                    sb.AppendLine($";Wartość minimalna;{value ?? '-'}");
                    break;
                case Aggregation.Max:
                    value = group.Max(t => (float?)t.Transaction.Amount);
                    sb.AppendLine($";Wartość maksymalna;{value ?? '-'}");
                    break;
            }
        }

        public static void WriteLoansData(this StringBuilder sb, IEnumerable<IGrouping<string, TransactionWithTimestamp>> groups, Loan[] loans)
        {
            foreach (var loan in loans)
            {
                sb.AppendLine($"Kredyt {loan.Id}:");
                foreach (var group in groups)
                {
                    var debits = group.Where(t => t.Transaction.PaymentId == loan.PaymentId).Sum(t => (float?)t.Transaction.Amount);
                    sb.AppendLine($";{group.Key}");
                    sb.AppendLine($";;spłacono;{debits ?? '-'}");
                }
            }
        }

        public static void WriteCardsData(this StringBuilder sb, IEnumerable<IGrouping<string, TransactionWithTimestamp>> groups, Card[] cards)
        {
            foreach (var card in cards)
            {
                sb.AppendLine($"Karta {card.Number}:");
                foreach (var group in groups)
                {
                    var debits = group.Where(t => t.Transaction.CardId == card.Id).Sum(t => (float?)t.Transaction.Amount);
                    sb.AppendLine($";{group.Key}");
                    sb.AppendLine($";;obciążenia;{debits ?? '-'}");
                }
            }
        }

        public static void WritePaymentsData(this StringBuilder sb, IEnumerable<IGrouping<string, TransactionWithTimestamp>> groups, Payment[] payments)
        {
            foreach (var payment in payments)
            {
                sb.AppendLine($"Płatność {payment}:");
                foreach (var group in groups)
                {
                    var debits = group.Where(t => t.Transaction.PaymentId == payment.Id).Sum(t => (float?)t.Transaction.Amount);
                    sb.AppendLine($";{group.Key}");
                    sb.AppendLine($";;obciążenia;{debits ?? '-'}");
                }
            }
        }

        public static void WriteAccountsData(this StringBuilder sb, IEnumerable<IGrouping<string, TransactionWithTimestamp>> groups, Account[] accounts)
        {
            foreach (var account in accounts)
            {
                sb.AppendLine($"Konto {account.Number}:");
                foreach (var group in groups)
                {
                    var incomes = group.Where(t => t.Transaction.Recipient == account.Id).Sum(t => (float?)t.Transaction.Amount) ?? 0;
                    var debits = group.Where(t => t.Transaction.Sender == account.Id).Sum(t => (float?)t.Transaction.Amount) ?? 0;
                    var balance = incomes - debits;

                    sb.AppendLine($";{group.Key}");
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
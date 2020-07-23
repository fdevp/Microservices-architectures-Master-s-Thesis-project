using System;
using System.Collections.Generic;
using System.Linq;

namespace SharedClasses
{
    public static class Aggregations
    {
        public static IEnumerable<OverallReportPortion> CreateOverallCsvReport(OverallReportData data)
        {
            var withTimestamps = data.Transactions.Select(t => new TransactionWithTimestamp { Timestamp = new DateTime(t.Timestamp), Transaction = t });
            var periods = GroupByPeriods(data.Granularity, withTimestamps);
            var ordered = periods.OrderBy(p => p.Key);
            foreach (var period in ordered)
            {
                foreach (var aggregation in data.Aggregations)
                {
                    var value = Aggregate(period, aggregation);
                    yield return new OverallReportPortion { Period = period.Key, Value = value, Aggregation = aggregation };
                }
            }
        }

        public static string GetDate(DateTime date, int day) => date.AddDays(-(int)date.DayOfWeek + day).ToString("yyyy-MM-dd");

        public static IEnumerable<IGrouping<string, TransactionWithTimestamp>> GroupByPeriods(Granularity granularity, IEnumerable<TransactionWithTimestamp> transactions)
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

        private static float Aggregate(IGrouping<string, TransactionWithTimestamp> period, Aggregation aggregation)
        {
            switch (aggregation)
            {
                case Aggregation.Count:
                    return period.Count();
                case Aggregation.Avg:
                    return period.Average(t => t.Transaction.Amount);
                case Aggregation.Median:
                    return (float)Median(period.Select(p => p.Transaction.Amount));
                case Aggregation.Min:
                    return period.Min(t => t.Transaction.Amount);
                case Aggregation.Max:
                    return period.Max(t => t.Transaction.Amount);
                default:
                    throw new InvalidOperationException("Unknown aggregation.");
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
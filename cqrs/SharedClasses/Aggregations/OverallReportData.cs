using System;

namespace SharedClasses
{
    public class OverallReportData
    {
        public Granularity Granularity { get; set; }
        public Aggregation[] Aggregations { get; set; }
        public Transaction[] Transactions { get; set; }
    }
}
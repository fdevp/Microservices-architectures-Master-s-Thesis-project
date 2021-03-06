using System;

namespace ReportsBranchMicroservice
{
    public class OverallReportData
    {
        public ReportSubject Subject { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public Granularity Granularity { get; set; }
        public Aggregation[] Aggregations { get; set; }
        public Transaction[] Transactions { get; set; }
    }
}
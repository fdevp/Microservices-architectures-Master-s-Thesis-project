using System;

namespace ReportsBranchMicroservice
{
    public class OverallReportData
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public Granularity Granularity { get; set; }
        public Aggregation[] Aggregations { get; set; }
    }
}
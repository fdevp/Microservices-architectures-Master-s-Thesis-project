using System;
using SharedClasses.Models;

namespace APIGateway.Models
{
    public class OverallReportRequest
    {
        public Aggregation[] Aggregations { get; set; }
        public DateTime? TimestampFrom { get; set; }
        public DateTime? TimestampTo { get; set; }
        public ReportGranularity Granularity { get; set; }
        public ReportSubject Subject { get; set; }
    }
}
using System;

namespace APIGateway.Models
{
    public class UserActivityReportRequest
    {
        public string UserId { get; set; }
        public DateTime? TimestampFrom { get; set; }
        public DateTime? TimestampTo { get; set; }
        public ReportGranularity Granularity { get; set; }
    }
}
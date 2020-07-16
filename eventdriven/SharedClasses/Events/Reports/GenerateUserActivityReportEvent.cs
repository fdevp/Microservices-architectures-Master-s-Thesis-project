using System;
using SharedClasses.Models;

namespace SharedClasses.Events.Reports
{
    public class GenerateUserActivityReportEvent
    {
        public string UserId { get; set; }
        public DateTime? TimestampFrom { get; set; }
        public DateTime? TimestampTo { get; set; }
        public Granularity Granularity { get; set; }
    }
}
using System;

namespace DataGenerator
{
    public class UserActivityReportScenarioElement
    {
        public string UserId { get; set; }
        public DateTime? TimestampFrom { get; set; }
        public DateTime? TimestampTo { get; set; }
        public int Granularity { get; set; }
    }
}
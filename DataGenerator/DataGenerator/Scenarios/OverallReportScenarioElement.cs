using System;

namespace DataGenerator
{
    public class OverallReportScenarioElement
    {
        public string No { get; set; }
        public int[] Aggregations { get; set; }
        public DateTime? TimestampFrom { get; set; }
        public DateTime? TimestampTo { get; set; }
        public int Granularity { get; set; }
        public int Subject { get; set; }
    }
}
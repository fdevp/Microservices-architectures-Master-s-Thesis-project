using System;
using SharedClasses.Models;

namespace SharedClasses.Events.Reports
{
    public class AggregatedOverallReportEvent
    {
        public OverallReportPortion[] Portions { get; set; }
    }
}
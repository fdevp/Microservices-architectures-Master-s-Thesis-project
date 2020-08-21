using System;
using SharedClasses.Models;

namespace SharedClasses.Events.Reports
{
    public class AggregatedUserActivityReportEvent
    {
        public UserReportPortion[] AccountsPortions { get; set; }
        public UserReportPortion[] PaymentsPortions { get; set; }
        public UserReportPortion[] LoansPortions { get; set; }
        public UserReportPortion[] CardsPortions { get; set; }
    }
}
using System;

namespace ReportsMicroservice
{
    public class UserActivityRaportData
    {
        public string UserId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public Granularity Granularity { get; set; }
        public UserActivityReportPortions Portions { get; set; }
    }

    public class UserActivityReportPortions
    {
        public UserReportPortion[] Accounts { get; set; }
        public UserReportPortion[] Payments { get; set; }
        public UserReportPortion[] Loans { get; set; }
        public UserReportPortion[] Cards { get; set; }
    }
}
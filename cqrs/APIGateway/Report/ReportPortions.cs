using System;

namespace APIGateway.Reports
{
    public class ReportPortions
    {
        public UserReportPortion[] Accounts { get; set; }
        public UserReportPortion[] Payments { get; set; }
        public UserReportPortion[] Loans { get; set; }
        public UserReportPortion[] Cards { get; set; }
    }
}
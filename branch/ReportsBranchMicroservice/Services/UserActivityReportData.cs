using System;

namespace ReportsBranchMicroservice
{
    public class UserActivityRaportData
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public Granularity Granularity { get; set; }
        public Account[] Accounts { get; set; }
        public Payment[] Payments { get; set; }
        public Transaction[] Transactions { get; set; }
        public Loan[] Loans { get; set; }
        public Card[] Cards { get; set; }
    }
}
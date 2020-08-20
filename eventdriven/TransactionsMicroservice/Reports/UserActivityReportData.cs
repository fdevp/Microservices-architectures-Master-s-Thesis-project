using System;
using SharedClasses.Models;

namespace TransactionsMicroservice.Reports
{
    public class UserActivityRaportData
    {
        public string UserId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public ReportGranularity Granularity { get; set; }
        public Account[] Accounts { get; set; }
        public Payment[] Payments { get; set; }
        public Transaction[] Transactions { get; set; }
        public Loan[] Loans { get; set; }
        public Card[] Cards { get; set; }
    }
}
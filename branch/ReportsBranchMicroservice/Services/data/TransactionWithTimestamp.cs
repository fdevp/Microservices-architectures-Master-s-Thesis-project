using System;

namespace ReportsBranchMicroservice
{
    public class TransactionWithTimestamp
    {
        public DateTime Timestamp { get; set; }
        public Transaction Transaction { get; set; }
    }
}
using System;

namespace SharedClasses
{
    public class TransactionWithTimestamp
    {
        public DateTime Timestamp { get; set; }
        public Transaction Transaction { get; set; }
    }
}
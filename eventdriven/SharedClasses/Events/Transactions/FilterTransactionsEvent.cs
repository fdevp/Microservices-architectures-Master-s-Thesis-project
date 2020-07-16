using System;

namespace SharedClasses.Events.Transactions
{
    public class FilterTransactionsEvent
    {
        public string[] Recipients { get; set; }
        public string[] Senders { get; set; }
        public string[] Payments { get; set; }
        public string[] Cards { get; set; }
        public DateTime? TimestampFrom { get; set; }
        public DateTime? TimestampTo { get; set; }
        public int? Top { get; set; }
    }
}
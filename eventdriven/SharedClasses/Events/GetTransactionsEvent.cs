using System;

namespace SharedClasses.Events
{
    public class GetTransactionsEvent
    {
        public string[] Ids { get; set; }

        public DateTime? TimestampFrom { get; set; }
        public DateTime? TimestampTo { get; set; }
    }
}
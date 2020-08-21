using System;
using System.Collections.Generic;

namespace TransactionsReadMicroservice.Repository
{
    public class Filters
    {
        public ISet<string> Recipients { get; set; }
        public ISet<string> Senders { get; set; }
        public ISet<string> Payments { get; set; }
        public ISet<string> Cards { get; set; }
        public DateTime? TimestampFrom { get; set; }
        public DateTime? TimestampTo { get; set; }
    }
}
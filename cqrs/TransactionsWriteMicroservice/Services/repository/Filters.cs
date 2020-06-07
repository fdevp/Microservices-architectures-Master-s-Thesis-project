using System;
using System.Collections.Generic;

namespace TransactionsWriteMicroservice.Repository
{
    public class Filters
    {
        public ISet<string> Recipients { get; set; }
        public ISet<string> Senders { get; set; }
        public ISet<string> Payments { get; set; }
        public ISet<string> Cards { get; set; }
        public long TimestampFrom { get; set; }
        public long TimestampTo { get; set; }
    }
}
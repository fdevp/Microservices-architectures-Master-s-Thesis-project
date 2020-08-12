using System;
using SharedClasses.Models;

namespace SharedClasses.Events.Payments
{
    public class UpdateRepayTimestampEvent
    {
        public string[] Ids { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
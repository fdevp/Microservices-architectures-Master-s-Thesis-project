using System;
using SharedClasses.Models;

namespace SharedClasses.Events.Payments
{
    public class UpdateLatestProcessingTimestampEvent
    {
        public string[] Ids { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
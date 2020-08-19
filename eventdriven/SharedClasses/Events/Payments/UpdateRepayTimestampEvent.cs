using System;
using SharedClasses.Models;

namespace SharedClasses.Events.Payments
{
    public class UpdateProcessingTimestampEvent
    {
        public string[] Ids { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
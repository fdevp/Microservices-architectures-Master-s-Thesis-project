using System;

namespace SharedClasses.Events.Payments
{
    public class CreatePaymentEvent
    {
        public string AccountId { get; set; }
        public string Recipient { get; set; }
        public float Amount { get; set; }
        public DateTime StartTimestamp { get; set; }
        public TimeSpan Interval { get; set; }
    }
}
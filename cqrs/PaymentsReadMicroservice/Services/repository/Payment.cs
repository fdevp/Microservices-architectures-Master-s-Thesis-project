using System;

namespace PaymentsReadMicroservice.Repository
{
    public class Payment
    {
        public string Id { get; set; }
        public float Amount { get; set; }
        public DateTime StartTimestamp { get; set; }
        public DateTime? ProcessingTimestamp { get; set; }
        public TimeSpan Interval { get; set; }
        public PaymentStatus Status { get; set; }
        public string AccountId { get; set; }
        public string Recipient { get; set; }
    }
}

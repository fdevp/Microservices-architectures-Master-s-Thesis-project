using System;

namespace APIGateway.Models
{
    public class PaymentDTO
    {
        public string Id { get; set; }
        public PaymentStatus Status { get; set; }
        public string AccountId { get; set; }
        public string Recipient { get; set; }
        public float Amount { get; set; }
        public DateTime StartTimestamp { get; set; }
        public TimeSpan Interval { get; set; }
    }
}
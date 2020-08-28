using System;

namespace DataGenerator
{
    public class PaymentDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Status { get; set; }
        public string AccountId { get; set; }
        public string Recipient { get; set; }
        public float Amount { get; set; }
        public DateTime StartTimestamp { get; set; }
        public DateTime LatestProcessingTimestamp { get; set; }
        public TimeSpan Interval { get; set; }
    }
}
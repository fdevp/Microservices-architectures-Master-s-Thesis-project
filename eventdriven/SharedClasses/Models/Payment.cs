using System;

namespace SharedClasses.Models
{
    public class Payment
    {
        public string Id { get; set; }
        public string Name {get;set;}
        public float Amount { get; set; }
        public DateTime StartTimestamp { get; set; }
        public DateTime LatestProcessingTimestamp { get; set; }
        public TimeSpan Interval { get; set; }
        public PaymentStatus Status { get; set; }
        public string AccountId { get; set; }
        public string Recipient { get; set; }

        public void Cancel()
        {
            this.Status = PaymentStatus.CANCELLED;
        }
    }
}

using System;

namespace PaymentsWriteMicroservice.Repository
{
    public class Payment
    {
        public string Id { get; }
        public float Amount { get; }
        public DateTime StartTimestamp { get; }
        public DateTime? LatestProcessingTimestamp { get; private set; }
        public TimeSpan Interval { get; }
        public PaymentStatus Status { get; private set; }
        public string AccountId { get; }
        public string Recipient { get; }

        public Payment(string id, float amount, DateTime startTimestamp, DateTime? latestProcessingTimestamp, TimeSpan interval, PaymentStatus status, string accountId, string recipient)
        {
            Id = id;
            Amount = amount;
            StartTimestamp = startTimestamp;
            LatestProcessingTimestamp = latestProcessingTimestamp;
            Interval = interval;
            Status = status;
            AccountId = accountId;
            Recipient = recipient;
        }

        public void UpdateLatestProcessingTimestamp(DateTime processingTimestamp)
        {
            this.LatestProcessingTimestamp = processingTimestamp;
        }

        public void Cancel()
        {
            this.Status = PaymentStatus.CANCELLED;
        }
    }
}

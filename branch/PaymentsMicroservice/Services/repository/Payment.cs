using System;

namespace PaymentsMicroservice.Repository
{
    public class Payment
    {
        public string Id { get; }
        public float Amount { get; }
        public DateTime StartTimestamp { get; }
        public DateTime? LastRepayTimestamp { get; private set; }
        public TimeSpan Interval { get; }
        public PaymentStatus Status { get; private set; }
        public string AccountId { get; }
        public string Recipient { get; }

        public Payment(string id, float amount, DateTime startTimestamp, DateTime? lastRepayTimestamp, TimeSpan interval, PaymentStatus status, string accountId, string recipient)
        {
            Id = id;
            Amount = amount;
            StartTimestamp = startTimestamp;
            LastRepayTimestamp = lastRepayTimestamp;
            Interval = interval;
            Status = status;
            AccountId = accountId;
            Recipient = recipient;
        }

        public void UpdateLastRepayTimestamp(DateTime lastRepayTimestamp)
        {
            this.LastRepayTimestamp = lastRepayTimestamp;
        }

        public void Cancel()
        {
            this.Status = PaymentStatus.CANCELLED;
        }
    }
}

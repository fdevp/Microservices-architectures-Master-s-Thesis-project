namespace PaymentsMicroservice.Repository
{
    public class Payment
    {
        public string Id { get; }
        public float Amount { get; }
        public long StartTimestamp { get; }
        public long LastRepayTimestamp { get; private set; }
        public long Interval { get; }
        public PaymentStatus Status { get; private set; }
        public string AccountId { get; }
        public string Recipient { get; }

        public Payment(string id, float amount, long startTimestamp, long lastRepayTimestamp, long interval, PaymentStatus status, string accountId, string recipient)
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

        public void UpdateLastRepayTimestamp(long lastRepayTimestamp)
        {
            this.LastRepayTimestamp = lastRepayTimestamp;
        }

        public void Cancel()
        {
            this.Status = PaymentStatus.CANCELLED;
        }
    }
}

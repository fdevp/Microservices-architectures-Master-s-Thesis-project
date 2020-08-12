namespace PaymentsReadMicroservice.Repository
{
    public class Payment
    {
        public string Id { get; set; }
        public float Amount { get; set; }
        public long StartTimestamp { get; set; }
        public long LastRepayTimestamp { get; set; }
        public long Interval { get; set; }
        public PaymentStatus Status { get; set; }
        public string AccountId { get; set; }
        public string Recipient { get; set; }
    }
}

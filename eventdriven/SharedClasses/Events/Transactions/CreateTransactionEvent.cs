namespace SharedClasses.Events.Transactions
{
    public class CreateTransactionEvent
    {
        public string Title { get; set; }
        public float Amount { get; set; }
        public string Recipient { get; set; }
        public string Sender { get; set; }
        public string PaymentId { get; set; }
        public string CardId { get; set; }
    }
}
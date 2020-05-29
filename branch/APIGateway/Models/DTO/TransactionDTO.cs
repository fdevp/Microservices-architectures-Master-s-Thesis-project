
namespace APIGateway.Models
{
    public class TransactionDTO
    {
        public string AccountId { get; set; }
        public string Title { get; set; }
        public string Recipient { get; set; }
        public string Sender { get; set; }
        public string PaymentId { get; set; }
        public string CardId { get; set; }
        public long Timestamp { get; set; }
        public float Amount { get; set; }
    }
}
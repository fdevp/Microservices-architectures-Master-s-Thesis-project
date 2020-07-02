
using System;

namespace APIGateway.Models
{
    public class TransactionDTO
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Recipient { get; set; }
        public string Sender { get; set; }
        public string PaymentId { get; set; }
        public string CardId { get; set; }
        public DateTime Timestamp { get; set; }
        public float Amount { get; set; }
    }
}
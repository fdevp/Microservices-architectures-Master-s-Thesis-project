using System;

namespace TransactionsMicroservice.Repository
{
    public class Transaction
    {
        public string Id { get; }
        public string Title { get; }
        public float Amount { get; }
        public DateTime Timestamp { get; }
        public string Recipient { get; }
        public string Sender { get; }
        public string PaymentId { get; }
        public string CardId { get; }

        public Transaction(string id, string title, float amount, DateTime timestamp, string recipient, string sender, string paymentId, string cardId)
        {
            Id = id;
            Title = title;
            Amount = amount;
            Timestamp = timestamp;
            Recipient = recipient;
            Sender = sender;
            PaymentId = paymentId;
            CardId = cardId;
        }
    }
}

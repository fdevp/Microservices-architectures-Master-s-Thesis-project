using System;

namespace TransactionsWriteMicroservice.Repository
{
    public class Transaction
    {
        public string Id { get; }
        public string Title { get; }
        public float Amount { get; }
        public long Timestamp { get; }
        public string Recipient { get; }
        public string Sender { get; }
        public string PaymentId { get; }
        public string CardId { get; }

        public Transaction(string id, string title, float amount, long timestamp, string recipient, string sender, string paymentId, string cardId)
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

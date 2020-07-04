using System;

namespace SharedClasses.Models.Cards
{
    public class Block
    {
        public string Id { get; }
        public string CardId { get; }
        public string TransactionId { get; }
        public DateTime Timestamp { get; }

        public Block(string id, string cardId, string transactionId, DateTime timestamp)
        {
            Id = id;
            CardId = cardId;
            TransactionId = transactionId;
            Timestamp = timestamp;
        }
    }
}
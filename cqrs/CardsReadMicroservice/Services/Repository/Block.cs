namespace CardsReadMicroservice.Repository
{
    public class Block
    {
        public string Id { get; }
        public string CardId { get; }
        public string TransactionId { get; }
        public long Timestamp { get; }

        public Block(string id, string cardId, string transactionId, long timestamp)
        {
            Id = id;
            CardId = cardId;
            TransactionId = transactionId;
            Timestamp = timestamp;
        }
    }
}
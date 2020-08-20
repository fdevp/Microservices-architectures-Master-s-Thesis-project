namespace CardsMicroservice.Repository
{
    public class Block
    {
        public string Id { get; set; }
        public string CardId { get; set; }
        public string TransactionId { get; set; }
        public long Timestamp { get; set; }
    }
}
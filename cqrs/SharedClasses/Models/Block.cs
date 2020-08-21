using System;

namespace Models
{
    public class Block
    {
        public string Id { get; set; }
        public string CardId { get; set; }
        public string TransactionId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
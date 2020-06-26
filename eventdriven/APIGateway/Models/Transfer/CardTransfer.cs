namespace APIGateway.Models
{
    public class CardTransfer
    {
        public string CardId { get; set; }
        public string Recipient { get; set; }
        public float Amount { get; set; }
    }
}
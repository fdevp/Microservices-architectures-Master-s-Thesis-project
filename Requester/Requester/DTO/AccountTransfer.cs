namespace Requester
{
    public class AccountTransfer
    {
        public string AccountId { get; set; }
        public string PaymentId { get; set; }
        public string Recipient { get; set; }
        public string Title { get; set; }
        public float Amount { get; set; }
    }
}
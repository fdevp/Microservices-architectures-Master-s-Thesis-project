using System;

namespace SharedClasses.Models
{
    public class Transfer
    {
        public string CardId { get; set; }
        public string PaymentId { get; set; }
        public string AccountId { get; set; }
        public string Recipient { get; set; }
        public float Amount { get; set; }
        public string Title { get; set; }
    }
}

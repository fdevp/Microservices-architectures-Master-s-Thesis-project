using System;

namespace SharedClasses.Models
{
    public class CardTransfer
    {
        public string CardId { get; set; }
        public string Recipient { get; set; }
        public float Amount { get; set; }
        public string Title { get; set; }
    }
}
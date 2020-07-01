using System;

namespace SharedClasses.Models
{
    public class Transfer
    {
        public string AccountId { get; }
        public string Recipient { get; }
        public float Amount { get; }
        public string Title { get; }
    }
}

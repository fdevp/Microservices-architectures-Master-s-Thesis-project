using System;
using System.Collections.Generic;
using System.Linq;

namespace TransactionsWriteMicroservice.Repository
{
    public class TransactionsRepository
    {
        private Dictionary<string, Models.Transaction> transactions = new Dictionary<string, Models.Transaction>();

        public Models.Transaction Create(string title, float amount, string recipient, string sender, string paymentId, string cardId)
        {
            var id = Guid.NewGuid().ToString();
            var timestamp = DateTime.UtcNow;
            var transaction = new Models.Transaction
            {
                Id = id,
                Title = title,
                Amount = amount,
                Timestamp = timestamp,
                Recipient = recipient,
                Sender = sender,
                PaymentId = paymentId,
                CardId = cardId
            };
            transactions.Add(id, transaction);
            return transaction;
        }

        public void Setup(IEnumerable<Models.Transaction> transactions)
        {
            this.transactions = transactions.ToDictionary(t => t.Id, t => t);
        }

        public void SetupAppend(IEnumerable<Models.Transaction> transactions)
        {
            if (this.transactions == null)
            {
                this.transactions = new Dictionary<string, Models.Transaction>();
            }

            foreach (var transaction in transactions)
            {
                this.transactions.Add(transaction.Id, transaction);
            }
        }
    }
}
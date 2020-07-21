using System;
using System.Collections.Generic;
using System.Linq;

namespace TransactionsWriteMicroservice.Repository
{
    public class TransactionsRepository
    {
        private Dictionary<string, Transaction> transactions = new Dictionary<string, Transaction>();

        public Transaction Create(string title, float amount, string recipient, string sender, string paymentId, string cardId)
        {
            var id = Guid.NewGuid().ToString();
            var timestamp = DateTime.UtcNow;
            var transaction = new Repository.Transaction(id, title, amount, timestamp.Ticks, recipient, sender, paymentId, cardId);
            transactions.Add(id, transaction);
            return transaction;
        }

        public void Setup(IEnumerable<Repository.Transaction> transactions)
        {
            this.transactions = transactions.ToDictionary(t => t.Id, t => t);
        }

        public void SetupAppend(IEnumerable<Repository.Transaction> transactions)
        {
            if (this.transactions == null)
            {
                this.transactions = new Dictionary<string, Transaction>();
            }

            foreach (var transaction in transactions)
            {
                this.transactions.Add(transaction.Id, transaction);
            }
        }
    }
}
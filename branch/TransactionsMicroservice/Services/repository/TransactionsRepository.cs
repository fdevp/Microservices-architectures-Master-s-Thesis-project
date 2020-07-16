using System;
using System.Collections.Generic;
using System.Linq;

namespace TransactionsMicroservice.Repository
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

        public Transaction Get(string id)
        {
            if (transactions.ContainsKey(id))
                return transactions[id];
            return null;
        }
        
        public Transaction[] GetMany(Filters filters, int top)
        {
            var selected = transactions.Values.Where(t => SelectTransaction(t, filters));
            if (top > 0)
                selected = selected.Take(top);
            return selected.ToArray();
        }

        public void Setup(IEnumerable<Repository.Transaction> transactions)
        {
            this.transactions = transactions.ToDictionary(t => t.Id, t => t);
        }

        private bool SelectTransaction(Transaction transaction, Filters filters)
        {
            if (filters.Payments.Any() && transaction.PaymentId != null && filters.Payments.Contains(transaction.PaymentId))
                return true;
            if (filters.Cards.Any() && transaction.CardId != null && filters.Cards.Contains(transaction.CardId))
                return true;
            if (filters.Recipients.Any() && transaction.Recipient != null && filters.Recipients.Contains(transaction.Recipient))
                return true;
            if (filters.Senders.Any() && transaction.Sender != null && filters.Senders.Contains(transaction.Sender))
                return true;
            if (filters.TimestampFrom > 0 && transaction.Timestamp >= filters.TimestampFrom)
                return true;
            if (filters.TimestampTo > 0 && transaction.Timestamp <= filters.TimestampTo)
                return true;

            return false;
        }
    }
}
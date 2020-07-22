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

        private bool SelectTransaction(Transaction transaction, Filters filters)
        {
            if (filters.TimestampFrom > 0 && transaction.Timestamp < filters.TimestampFrom)
                return false;
            if (filters.TimestampTo > 0 && transaction.Timestamp > filters.TimestampTo)
                return false;

            if (filters.Payments.Any() && !string.IsNullOrEmpty(transaction.PaymentId) && filters.Payments.Contains(transaction.PaymentId))
                return true;
            if (filters.Cards.Any() && !string.IsNullOrEmpty(transaction.CardId) && filters.Cards.Contains(transaction.CardId))
                return true;
            if (filters.Recipients.Any() && filters.Recipients.Contains(transaction.Recipient))
                return true;
            if (filters.Senders.Any() && filters.Senders.Contains(transaction.Sender))
                return true;

            var anyDetailedFilter = filters.Payments.Any() || filters.Cards.Any() || filters.Recipients.Any() || filters.Senders.Any();
            if (!anyDetailedFilter && filters.TimestampFrom == 0 && filters.TimestampTo == 0)
                return true;

            return false;
        }
    }
}
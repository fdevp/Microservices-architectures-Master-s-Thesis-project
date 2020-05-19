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

        public Transaction[] GetMany(Filters filters)
        {
            return transactions.Values.Where(t => SelectTransaction(t, filters)).ToArray();
        }

        public void Setup(IEnumerable<Transaction> transactions)
        {
            this.transactions = transactions.ToDictionary(t => t.Id, t => t);
        }

        public void TearDown()
        {
            this.transactions = new Dictionary<string, Transaction>();
        }

        private bool SelectTransaction(Transaction transaction, Filters filters)
        {
            if (filters.Payments.Any() && transaction.PaymentId != null && !filters.Payments.Contains(transaction.PaymentId))
                return false;
            if (filters.Cards.Any() && transaction.CardId != null && !filters.Cards.Contains(transaction.CardId))
                return false;
            if (filters.Recipients.Any() && transaction.Recipient != null && !filters.Payments.Contains(transaction.Recipient))
                return false;
            if (filters.Senders.Any() && transaction.Sender != null && !filters.Payments.Contains(transaction.Sender))
                return false;
            if (filters.TimestampFrom != null && transaction.Timestamp < filters.TimestampFrom.Ticks)
                return false;
            if (filters.TimestampTo != null && transaction.Timestamp > filters.TimestampTo.Ticks)
                return false;

            return true;
        }
    }
}
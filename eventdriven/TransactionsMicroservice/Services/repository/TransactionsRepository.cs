using System;
using System.Collections.Generic;
using System.Linq;
using SharedClasses.Models;

namespace TransactionsMicroservice.Repository
{
    public class TransactionsRepository
    {
        private Dictionary<string, Transaction> transactions = new Dictionary<string, Transaction>();

        public Transaction Create(string title, float amount, string recipient, string sender, string paymentId, string cardId)
        {
            var id = Guid.NewGuid().ToString();
            var timestamp = DateTime.UtcNow;
            var transaction = new Transaction
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
        
        private bool SelectTransaction(Transaction transaction, Filters filters)
        {
            if (filters.Payments != null && filters.Payments.Any() && transaction.PaymentId != null && filters.Payments.Contains(transaction.PaymentId))
                return true;
            if (filters.Cards != null && filters.Cards.Any() && transaction.CardId != null && filters.Cards.Contains(transaction.CardId))
                return true;
            if (filters.Recipients != null && filters.Recipients.Any() && transaction.Recipient != null && filters.Recipients.Contains(transaction.Recipient))
                return true;
            if (filters.Senders != null && filters.Senders.Any() && transaction.Sender != null && filters.Senders.Contains(transaction.Sender))
                return true;
            if (filters.TimestampFrom != null && transaction.Timestamp >= filters.TimestampFrom)
                return true;
            if (filters.TimestampTo != null && transaction.Timestamp <= filters.TimestampTo)
                return true;

            return false;
        }
    }
}
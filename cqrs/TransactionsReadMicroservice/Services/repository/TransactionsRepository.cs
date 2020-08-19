using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace TransactionsReadMicroservice.Repository
{
    public class TransactionsRepository
    {
        private ConcurrentDictionary<string, Models.Transaction> transactions = new ConcurrentDictionary<string, Models.Transaction>();

        public Models.Transaction Get(string id)
        {
            if (transactions.ContainsKey(id))
                return transactions[id];
            return null;
        }

        public Models.Transaction[] GetMany(Filters filters, int top)
        {
            var selected = transactions.Values.Where(t => SelectTransaction(t, filters));
            if (top > 0)
                selected = selected.Take(top);
            return selected.ToArray();
        }

        public void Upsert(Models.Transaction[] update)
        {
            foreach (var transaction in update)
            {
                transactions[transaction.Id] = transaction;
            }
        }

        public void Remove(string[] ids)
        {
            foreach (var id in ids)
            {
                transactions.Remove(id, out var transaction);
            }
        }

        private bool SelectTransaction(Models.Transaction transaction, Filters filters)
        {
			 if (filters.TimestampFrom.HasValue && transaction.Timestamp < filters.TimestampFrom)
                return false;
            if (filters.TimestampTo.HasValue && transaction.Timestamp > filters.TimestampTo)
                return false;
			
            var anyPayments = filters.Payments?.Any() ?? false;
            if (anyPayments && !string.IsNullOrEmpty(transaction.PaymentId) && filters.Payments.Contains(transaction.PaymentId))
                return true;

            var anyCards = filters.Cards?.Any() ?? false;
            if (anyCards && !string.IsNullOrEmpty(transaction.CardId) && filters.Cards.Contains(transaction.CardId))
                return true;

            var anyRecipients = filters.Recipients?.Any() ?? false;
            if (anyRecipients && filters.Recipients.Contains(transaction.Recipient))
                return true;

            var anySenders = filters.Senders?.Any() ?? false;
            if (anySenders && filters.Senders.Contains(transaction.Sender))
                return true;

            var anyDetailedFilter = anyPayments || anyCards || anyRecipients || anySenders;
            if (anyDetailedFilter)
                return false;

            if (filters.TimestampFrom.HasValue && filters.TimestampTo.HasValue)
                return transaction.Timestamp >= filters.TimestampFrom && transaction.Timestamp <= filters.TimestampTo;
            if (filters.TimestampTo.HasValue)
                return transaction.Timestamp <= filters.TimestampTo;
            if (filters.TimestampFrom.HasValue)
                return transaction.Timestamp >= filters.TimestampFrom;

            return false;
        }
    }
}
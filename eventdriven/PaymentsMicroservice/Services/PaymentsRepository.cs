using System;
using System.Linq;
using System.Collections.Generic;
using SharedClasses.Models;

namespace PaymentsMicroservice.Repository
{
    public class PaymentsRepository
    {
        private Dictionary<string, Payment> payments = new Dictionary<string, Payment>();

        public Payment Get(string id)
        {
            if (payments.ContainsKey(id))
                return payments[id];
            return null;
        }

        public Payment[] Get(int part, int totalParts)
        {
            return payments.Values.Where((element, index) => ((index % totalParts) + 1) == part).ToArray();
        }

        public Payment[] GetByAccounts(IEnumerable<string> accountIds)
        {
            var accountsSet = accountIds.ToHashSet();
            return payments.Values.Where(p => accountsSet.Contains(p.AccountId)).ToArray();
        }

        public Payment Create(float amount, DateTime startTimestamp, TimeSpan interval, string accountId, string recipient)
        {
            var payment = new Payment
            {
                Id = Guid.NewGuid().ToString(),
                Amount = amount,
                StartTimestamp = startTimestamp,
                Interval = interval,
                Status = PaymentStatus.ACTIVE,
                AccountId = accountId,
                Recipient = recipient
            };

            payments.Add(payment.Id, payment);
            return payment;
        }

        public void Cancel(string id)
        {
            if (payments.ContainsKey(id))
                payments[id].Cancel();
        }

        public void Setup(IEnumerable<Payment> payments)
        {
            this.payments = payments.ToDictionary(p => p.Id, p => p);
        }
    }
}
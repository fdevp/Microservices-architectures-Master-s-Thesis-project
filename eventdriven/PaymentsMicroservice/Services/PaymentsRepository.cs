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

        public Payment[] Get(int part, int totalParts, DateTime dateTime)
        {
            return payments.Values
                .Where((element, index) => ((index % totalParts) + 1) == part)
                .Where((element, index) => element.LatestProcessingTimestamp + element.Interval < dateTime)
                .ToArray();
        }

        public string[] GetIds() => payments.Values.Select(p => p.Id).ToArray();

        public Payment[] GetByAccounts(IEnumerable<string> accountIds)
        {
            var accountsSet = accountIds.ToHashSet();
            return payments.Values.Where(p => accountsSet.Contains(p.AccountId)).ToArray();
        }

        public void UpdateLatestProcessingTimestamp(IEnumerable<string> paymentsIds, DateTime processingTimestamp)
        {
            foreach (var id in paymentsIds)
                payments[id].LatestProcessingTimestamp = processingTimestamp;
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

        public void SetupAppend(IEnumerable<Payment> payments)
        {
            if (this.payments == null)
            {
                this.payments = new Dictionary<string, Payment>();
            }

            foreach (var payment in payments)
            {
                this.payments.TryAdd(payment.Id, payment);
            }
        }
    }
}
using System;
using System.Linq;
using System.Collections.Generic;

namespace PaymentsWriteMicroservice.Repository
{
    public class PaymentsRepository
    {
        private Dictionary<string, Models.Payment> payments = new Dictionary<string, Models.Payment>();

        public Models.Payment Get(string id)
        {
            if (payments.ContainsKey(id))
                return payments[id];
            return null;
        }

        public Models.Payment[] Get(int part, int totalParts)
        {
            return payments.Values.Where((element, index) => ((index % totalParts) + 1) == part).ToArray();
        }

        public Models.Payment[] GetByAccounts(IEnumerable<string> accountIds)
        {
            var accountsSet = accountIds.ToHashSet();
            return payments.Values.Where(p => accountsSet.Contains(p.AccountId)).ToArray();
        }

        public void UpdateLastRepayTimestamp(IEnumerable<string> paymentsIds, DateTime latestProcessingTimestmap)
        {
            foreach (var id in paymentsIds)
                payments[id].LatestProcessingTimestamp = latestProcessingTimestmap;
        }

        public Models.Payment Create(float amount, DateTime startTimestamp, TimeSpan interval, string accountId, string recipient)
        {
            var payment = new Models.Payment
            {
                Id = Guid.NewGuid().ToString(),
                Amount = amount,
                StartTimestamp = startTimestamp,
                LatestProcessingTimestamp = null,
                Interval = interval,
                Status = Models.PaymentStatus.ACTIVE,
                AccountId = accountId,
                Recipient = recipient
            };
            payments.Add(payment.Id, payment);
            return payment;
        }

        public void Cancel(string id)
        {
            if (payments.ContainsKey(id))
                payments[id].Status = Models.PaymentStatus.CANCELLED;
        }

        public void Setup(IEnumerable<Models.Payment> payments)
        {
            this.payments = payments.ToDictionary(p => p.Id, p => p);
        }

        public void SetupAppend(IEnumerable<Models.Payment> payments)
        {
            if (this.payments == null)
            {
                this.payments = new Dictionary<string, Models.Payment>();
            }

            foreach (var payment in payments)
            {
                this.payments.TryAdd(payment.Id, payment);
            }
        }
    }
}
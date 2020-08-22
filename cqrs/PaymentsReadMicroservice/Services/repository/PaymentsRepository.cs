using System;
using System.Linq;
using System.Collections.Generic;

namespace PaymentsReadMicroservice.Repository
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

        public string[] GetIds() => payments.Select(p => p.Value.Id).ToArray();

        public Models.Payment[] Get(int part, int totalParts, DateTime dateTime)
        {
            return payments.Values
                .Where((element, index) => ((index % totalParts) + 1) == part)
                .Where((element, index) => element.LatestProcessingTimestamp + element.Interval < dateTime)
                .ToArray();
        }

        public Models.Payment[] GetByAccounts(IEnumerable<string> accountIds)
        {
            var accountsSet = accountIds.ToHashSet();
            return payments.Values.Where(p => accountsSet.Contains(p.AccountId)).ToArray();
        }

        public void Upsert(Models.Payment[] update)
        {
            foreach (var payment in update)
            {
                payments[payment.Id] = payment;
            }
        }

        public void Remove(string[] ids)
        {
            foreach (var id in ids)
            {
                payments.Remove(id);
            }
        }

        public void Clear()
        {
            this.payments = new Dictionary<string, Models.Payment>();
        }
    }
}
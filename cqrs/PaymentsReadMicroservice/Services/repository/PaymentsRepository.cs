using System;
using System.Linq;
using System.Collections.Generic;

namespace PaymentsReadMicroservice.Repository
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

        public string[] GetIds() => payments.Select(p => p.Value.Id).ToArray();

        public Payment[] Get(int part, int totalParts)
        {
            return payments.Values.Where((element, index) => ((index % totalParts) + 1) == part).ToArray();
        }

        public Payment[] GetByAccounts(IEnumerable<string> accountIds)
        {
            var accountsSet = accountIds.ToHashSet();
            return payments.Values.Where(p => accountsSet.Contains(p.AccountId)).ToArray();
        }

        public void Upsert(Payment[] update)
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
    }
}
using System;
using System.Collections.Generic;

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

        public Payment Create(float amount, long startTimestamp, long interval, string accountId, string recipient)
        {
            var payment = new Repository.Payment(Guid.NewGuid().ToString(), amount, startTimestamp, interval, PaymentStatus.ACTIVE, accountId, recipient);
            payments.Add(payment.Id, payment);
            return payment;
        }

        public void Cancel(string id)
        {
            if (payments.ContainsKey(id))
                payments[id].Cancel();
        }
    }
}
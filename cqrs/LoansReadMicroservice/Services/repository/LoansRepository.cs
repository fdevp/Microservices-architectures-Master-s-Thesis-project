using System.Collections.Generic;
using System.Linq;

namespace LoansReadMicroservice.Repository
{
    public class LoansRepository
    {
        private Dictionary<string, Loan> loans = new Dictionary<string, Loan>();

        public Loan Get(string id)
        {
            if (loans.ContainsKey(id))
                return loans[id];
            return null;
        }

        public Loan[] GetAll() => loans.Values.ToArray();

        public string[] GetPaymentsIds() => loans.Select(l => l.Value.PaymentId).ToArray();

        public Loan[] GetByPayments(IEnumerable<string> paymentIds)
        {
            var payments = paymentIds.ToHashSet();
            return loans.Values.Where(l => payments.Contains(l.PaymentId)).ToArray();
        }

        public Loan[] GetByAccounts(IEnumerable<string> accountsIds)
        {
            var accounts = accountsIds.ToHashSet();
            return loans.Values.Where(l => accounts.Contains(l.AccountId)).ToArray();
        }

        public void Upsert(Loan[] update)
        {
            foreach (var loan in update)
            {
                loans[loan.Id] = loan;
            }
        }

        public void Remove(string[] ids)
        {
            foreach (var id in ids)
            {
                loans.Remove(id);
            }
        }
    }
}
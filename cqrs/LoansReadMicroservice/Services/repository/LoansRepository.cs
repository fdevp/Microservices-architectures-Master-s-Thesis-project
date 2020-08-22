using System.Collections.Generic;
using System.Linq;

namespace LoansReadMicroservice.Repository
{
    public class LoansRepository
    {
        private Dictionary<string, Models.Loan> loans = new Dictionary<string, Models.Loan>();

        public Models.Loan Get(string id)
        {
            if (loans.ContainsKey(id))
                return loans[id];
            return null;
        }

        public Models.Loan[] GetAll() => loans.Values.ToArray();

        public string[] GetPaymentsIds() => loans.Select(l => l.Value.PaymentId).ToArray();

        public Models.Loan[] GetByPayments(IEnumerable<string> paymentIds)
        {
            var payments = paymentIds.ToHashSet();
            return loans.Values.Where(l => payments.Contains(l.PaymentId)).ToArray();
        }

        public Models.Loan[] GetByAccounts(IEnumerable<string> accountsIds)
        {
            var accounts = accountsIds.ToHashSet();
            return loans.Values.Where(l => accounts.Contains(l.AccountId)).ToArray();
        }

        public void Upsert(Models.Loan[] update)
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

        public void Clear()
        {
            this.loans = new Dictionary<string, Models.Loan>();
        }
    }
}
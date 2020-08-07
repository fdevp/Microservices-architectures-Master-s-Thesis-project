using System.Collections.Generic;
using System.Linq;

namespace LoansMicroservice.Repository
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

        public bool RepayInstalment(string id)
        {
            var loan = loans[id];
            var amount = InstalmentAmount(loan);
            loan.Repay(amount);
            if (loan.PaidAmount >= loan.TotalAmount)
                return true;
            return false;
        }

        private float InstalmentAmount(Loan loan)
        {
            var regularAmount = loan.TotalAmount / loan.Instalments;
            var toRepay = loan.TotalAmount - loan.PaidAmount;
            if (regularAmount > toRepay)
                return toRepay;
            return regularAmount;
        }

        public void Setup(IEnumerable<Repository.Loan> loans)
        {
            this.loans = loans.ToDictionary(l => l.Id, l => l);
        }

        public void SetupAppend(IEnumerable<Repository.Loan> loans)
        {
            if (this.loans == null)
            {
                this.loans = new Dictionary<string, Loan>();
            }

            foreach (var loan in loans)
            {
                this.loans.TryAdd(loan.Id, loan);
            }
        }
    }
}
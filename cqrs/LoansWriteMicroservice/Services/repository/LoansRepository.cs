using System.Collections.Generic;
using System.Linq;

namespace LoansWriteMicroservice.Repository
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

        public Models.Loan[] GetByPayment(IEnumerable<string> paymentIds)
        {
            var payments = paymentIds.ToHashSet();
            return loans.Values.Where(l => payments.Contains(l.PaymentId)).ToArray();
        }

        public bool RepayInstalment(string id)
        {
            var loan = loans[id];
            var amount = InstalmentAmount(loan);
            loan.PaidAmount += amount;
            if (loan.PaidAmount >= loan.TotalAmount)
                return true;
            return false;
        }

        private float InstalmentAmount(Models.Loan loan)
        {
            var regularAmount = loan.TotalAmount / loan.Instalments;
            var toRepay = loan.TotalAmount - loan.PaidAmount;
            if (regularAmount > toRepay)
                return toRepay;
            return regularAmount;
        }

        public void Setup(IEnumerable<Models.Loan> loans)
        {
            this.loans = loans.ToDictionary(l => l.Id, l => l);
        }

        public void SetupAppend(IEnumerable<Models.Loan> loans)
        {
            if (this.loans == null)
            {
                this.loans = new Dictionary<string, Models.Loan>();
            }

            foreach (var loan in loans)
            {
                this.loans.TryAdd(loan.Id, loan);
            }
        }
    }
}
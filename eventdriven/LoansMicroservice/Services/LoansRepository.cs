using System.Collections.Generic;
using System.Linq;
using SharedClasses.Models;

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

        public Loan[] GetByPayment(IEnumerable<string> paymentIds)
        {
            var payments = paymentIds.ToHashSet();
            return loans.Values.Where(l=> payments.Contains(l.PaymentId)).ToArray();
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

        public void Setup(IEnumerable<Loan> loans)
        {
            this.loans = loans.ToDictionary(l => l.Id, l => l);
        }

        public void TearDown()
        {
            this.loans = new Dictionary<string, Loan>();
        }
    }
}
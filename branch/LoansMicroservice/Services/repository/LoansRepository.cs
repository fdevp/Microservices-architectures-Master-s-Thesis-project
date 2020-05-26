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
            var toRepay = loan.PaidAmount - loan.TotalAmount;
            if (regularAmount > toRepay)
                return toRepay;
            return regularAmount;
        }

        public void Setup(IEnumerable<Repository.Loan> loans)
        {
            this.loans = loans.ToDictionary(l => l.Id, l => l);
        }

        public void TearDown()
        {
            this.loans = new Dictionary<string, Loan>();
        }
    }
}
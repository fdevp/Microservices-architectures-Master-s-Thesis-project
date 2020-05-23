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

        public float InstalmentAmount(string id)
        {
            var loan = loans[id];
            var regularAmount = loan.TotalAmount / loan.Instalments;
            var toRepay = loan.PaidAmount - loan.TotalAmount;
            if (regularAmount > toRepay)
                return toRepay;
            return regularAmount;
        }

        public void RepayInstalment(string id, float amount)
        {
            var loan = loans[id];
            loan.Repay(amount);
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
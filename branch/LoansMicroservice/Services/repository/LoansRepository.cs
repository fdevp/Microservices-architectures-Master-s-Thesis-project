using System.Collections.Generic;

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
            //return loan.
        }

        public void Setup(IEnumerable<Repository.Laon> loans)
        {
            this.loans = loans.ToDictionary(l => l.Id, l => l);
        }

        public void TearDown()
        {
            this.loans = new Dictionary<string, Loan>();
        }
    }
}
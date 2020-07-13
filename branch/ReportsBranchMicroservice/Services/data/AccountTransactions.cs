using System.Collections.Generic;
using System.Linq;

namespace ReportsBranchMicroservice
{
    public class AccountTransactions
    {
        public string AccountId { get; }
        public Transaction[] Transactions { get; }

        public AccountTransactions(string accountId, IEnumerable<Transaction> transactions)
        {
            AccountId = accountId;
            Transactions = transactions.ToArray();
        }
    }
}
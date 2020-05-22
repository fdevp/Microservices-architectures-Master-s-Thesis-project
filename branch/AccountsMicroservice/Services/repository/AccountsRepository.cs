using System;
using System.Collections.Generic;
using System.Linq;

namespace AccountsMicroservice.Repository
{
    public class AccountsRepository
    {
        private Dictionary<string, Account> accounts = new Dictionary<string, Account>();

        public Account Get(string id)
        {
            if (accounts.ContainsKey(id))
                return accounts[id];
            return null;
        }

        public void ChangeBalance(string id, float amount)
        {
            if (!accounts.ContainsKey(id))
                throw new ArgumentException("Account not found.");
            var account = accounts[id];
            var newBalance = account.Balance + amount;
            account.SetBalance(newBalance);
        }

        public void Setup(IEnumerable<Repository.Account> accounts)
        {
            this.accounts = accounts.ToDictionary(a => a.Id, a => a);
        }

        public void TearDown()
        {
            this.accounts = new Dictionary<string, Account>();
        }
    }
}
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

        public Account[] GetByUser(string userId)
        {
            return accounts.Values.Where(a=>a.UserId == userId).ToArray();
        }

        public bool CanTransfer(string sender, string recipient, float amount)
        {
            if (!accounts.ContainsKey(sender))
                return false;
            if (!accounts.ContainsKey(recipient))
                return false;
            if (accounts[sender].Balance < amount)
                return false;
            return true;
        }

        public void Transfer(string sender, string recipient, float amount)
        {
            if (!accounts.ContainsKey(sender))
                throw new ArgumentException("Account not found.");
            if (!accounts.ContainsKey(recipient))
                throw new ArgumentException("Recipient not found.");
            if (accounts[sender].Balance < amount)
                throw new ArgumentException("Not enough founds on the account.");

            ChangeBalance(recipient, amount);
            ChangeBalance(sender, amount * (-1));
        }

        private void ChangeBalance(string id, float amount)
        {
            var account = accounts[id];
            account.Balance = account.Balance + amount;
        }

        public void Setup(IEnumerable<Repository.Account> accounts)
        {
            this.accounts = accounts.ToDictionary(a => a.Id, a => a);
        }
        
        public void SetupAppend(IEnumerable<Repository.Account> accounts)
        {
            if (this.accounts == null)
            {
                this.accounts = new Dictionary<string, Account>();
            }

            foreach (var account in accounts)
            {
                this.accounts.TryAdd(account.Id, account);
            }
        }
    }
}
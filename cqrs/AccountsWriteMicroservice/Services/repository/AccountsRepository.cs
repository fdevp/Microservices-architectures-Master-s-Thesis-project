using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AccountsWriteMicroservice.Repository
{
    public class AccountsRepository
    {
        private ConcurrentDictionary<string, Models.Account> accounts = new ConcurrentDictionary<string, Models.Account>();

        public Models.Account Get(string id)
        {
            if (accounts.ContainsKey(id))
                return accounts[id];
            return null;
        }

        public Models.Account[] GetByUser(string userId)
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

        public void Setup(IEnumerable<Models.Account> accounts)
        {
            this.accounts = new ConcurrentDictionary<string, Models.Account>(accounts.ToDictionary(a => a.Id, a => a));
        }

        public void SetupAppend(IEnumerable<Models.Account> accounts)
        {
            if (this.accounts == null)
            {
                this.accounts = new ConcurrentDictionary<string, Models.Account>();
            }

            foreach (var account in accounts)
            {
                this.accounts.TryAdd(account.Id, account);
            }
        }
    }
}
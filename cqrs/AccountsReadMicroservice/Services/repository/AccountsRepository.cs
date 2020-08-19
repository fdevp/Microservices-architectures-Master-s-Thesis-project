using System;
using System.Collections.Generic;
using System.Linq;

namespace AccountsReadMicroservice.Repository
{
    public class AccountsRepository
    {
        private Dictionary<string, Models.Account> accounts = new Dictionary<string, Models.Account>();

        public Models.Account Get(string id)
        {
            if (accounts.ContainsKey(id))
                return accounts[id];
            return null;
        }

        public Models.Account[] GetByUser(string userId)
        {
            return accounts.Values.Where(a => a.UserId == userId).ToArray();
        }

        public void Upsert(Models.Account[] update)
        {
            foreach (var account in update)
            {
                accounts[account.Id] = account;
            }
        }

        public void Remove(string[] ids)
        {
            foreach (var id in ids)
            {
                accounts.Remove(id);
            }
        }
    }
}
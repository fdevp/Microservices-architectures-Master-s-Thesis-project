using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AccountsReadMicroservice.Repository
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
                accounts.TryRemove(id, out var removed);
            }
        }

        public void Clear()
        {
            this.accounts = new ConcurrentDictionary<string, Models.Account>();
        }
    }
}
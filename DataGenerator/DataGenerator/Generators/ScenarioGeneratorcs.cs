using DataGenerator.DTO;
using DataGenerator.Rnd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataGenerator
{
    public static class ScenarioGenerator
    {
        static Random rand = new Random();

        public static string GenerateIndividualUser(SetupAll setup)
        {
            int clients = 40;
            int actionsPerGroup = 20000;

            var users = setup.UsersSetup.Users.Where(u => !u.Business)
                .Select((u, i) => new { index = i, user = u })
                .ToArray();
            var groups = users.GroupBy(u => u.index % clients);
            var cardsAccounts = setup.CardsSetup.Cards.GroupBy(c=>c.AccountId).ToDictionary(k => k.Key, v => v.First());
            var accounts = setup.AccountsSetup.Accounts.Where(a => cardsAccounts.ContainsKey(a.Id))
                .GroupBy(a => a.UserId)
                .ToDictionary(k => k.Key, v => v.ToArray());

            var amountRnd = new RndBuilder<float>(new CurrencyRnd()).Min(5).Max(5000)
                .DistributionValues(new float[] { 20, 100, 300, 500, 1000, 2000, 3000 })
                .DistributionProbabilities(new[] { 35, 30, 20, 5, 5, 2, 1, 1 })
                .Build(); //dystrybuanta - max bardzo rzadko, min bardzo często

            var actions = new List<string>(actionsPerGroup * clients);
            int groupIndex = 0;
            foreach (var group in groups)
            {
                var groupAccounts = group.Where(g => accounts.ContainsKey(g.user.Id)).SelectMany(g => accounts[g.user.Id]).ToArray();
                var groupAccountsByUser = groupAccounts.GroupBy(a => a.UserId).ToDictionary(k => k.Key, v => v.ToArray());

                for (int i = 0; i < actionsPerGroup; i++)
                {
                    var amount = amountRnd.Next();
                    var sender = GetSender(amount, groupAccountsByUser);
                    var card = cardsAccounts[sender.Id];
                    var recipient = groupAccounts.ElementAt(rand.Next(0, groupAccounts.Length));
                    var obj = "{" + $"'group':{groupIndex},'amount':{amount},'cardId':'{card.Id}',recipient:'{recipient.Id}'" + "}";
                    actions.Add(obj);

                    sender.Balance -= amount;
                    recipient.Balance += amount;
                }

                groupIndex++;
            }

            return $"[{string.Join(",", actions)}]";
        }



        private static string GenerateBusinessUser(SetupAll setup)
        {
            var sb = new StringBuilder();

            return sb.ToString();
        }

        private static AccountDTO GetSender(float amount, Dictionary<string, AccountDTO[]> accounts)
        {
            while (true)
            {
                var randPortion = accounts.ElementAt(rand.Next(0, accounts.Count));
                var sender = randPortion.Value.FirstOrDefault(a => a.Balance > amount);
                if (sender != null)
                    return sender;
            }
        }
    }
}

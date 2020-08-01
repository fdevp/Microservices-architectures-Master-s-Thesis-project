using DataGenerator.DTO;
using DataGenerator.Rnd;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DataGenerator
{
    public static class ScenarioGenerator
    {
        static Random rand = new Random();

        public static string IndividualUserScenario(SetupAll setup, int clients, int actionsPerGroup)
        {
            var users = setup.UsersSetup.Users.Where(u => !u.Business)
                .Select((u, i) => new { index = i, user = u })
                .ToArray();
            var usersDict = users.ToDictionary(k => k.user.Id, v => v.user);
            var groups = users.GroupBy(u => u.index % clients);
            var cardsAccounts = setup.CardsSetup.Cards.GroupBy(c => c.AccountId).ToDictionary(k => k.Key, v => v.First());
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
                var usersAccounts = groupAccounts.GroupBy(a => a.UserId).ToDictionary(k => k.Key, v => v.ToArray());

                for (int i = 0; i < actionsPerGroup; i++)
                {
                    var amount = (float)Math.Round(amountRnd.Next(), 2);
                    var sender = GetSender(amount, usersAccounts);
                    var card = cardsAccounts[sender.Id];
                    var user = usersDict[sender.UserId].Login;
                    var recipient = groupAccounts.ElementAt(rand.Next(0, groupAccounts.Length));
                    var obj = "{" + $"'group':{groupIndex},'user':'{user}','amount':{amount.ToString(CultureInfo.InvariantCulture)},'cardId':'{card.Id}',recipient:'{recipient.Id}'" + "}";
                    actions.Add(obj);

                    sender.Balance -= amount;
                    recipient.Balance += amount;
                }

                groupIndex++;
            }

            return $"[{string.Join(",", actions)}]";
        }
        
        public static string BusinessUserScenario(SetupAll setup, int clients, int minUserActions, int maxUserActions)
        {
            var users = setup.UsersSetup.Users.Where(u => !u.Business)
                .Select((u, i) => new { index = i, user = u })
                .ToArray();
            var usersDict = users.ToDictionary(k => k.user.Id, v => v.user);
            var groups = users.GroupBy(u => u.index % clients);
            var accounts = setup.AccountsSetup.Accounts
                .GroupBy(a => a.UserId)
                .ToDictionary(k => k.Key, v => v.ToArray());

            var minAmount = 50;
            var amountRnd = new RndBuilder<float>(new CurrencyRnd()).Min(minAmount).Max(10000)
                .DistributionValues(new float[] { 300, 500, 1500, 2500, 5000, 8000 })
                .DistributionProbabilities(new[] { 2, 15, 20, 30, 25, 5, 3 })
                .Build();
            var titelRnd = new TitleRnd();
            var userActionsRnd = new RndBuilder<int>().Min(minUserActions).Max(maxUserActions).Build();

            var sb = new StringBuilder();
            var groupActions = new List<string>();
            int groupIndex = 0;
            foreach (var group in groups)
            {
                var groupAccounts = group.Where(g => accounts.ContainsKey(g.user.Id)).SelectMany(g => accounts[g.user.Id]).ToArray();
                var usersAccounts = groupAccounts.GroupBy(a => a.UserId).ToDictionary(k => k.Key, v => v.ToArray());


                foreach (var user in group)
                {
                    var userAccounts = usersAccounts[user.user.Id];
                    var userActionsCount = userActionsRnd.Next();
                    var userActions = new List<string>();

                    sb.Append("{" + $"'group':{groupIndex},'user':'{user.user.Login}', 'userId':'{user.user.Id}', 'transactions':");
                    for (int i = 0; i < userActionsCount; i++)
                    {
                        if (userAccounts.All(a => a.Balance < minAmount))
                            continue;
                        var amount = GetAmount(userAccounts, amountRnd);
                        var sender = GetSender(amount, userAccounts);
                        var recipient = groupAccounts.ElementAt(rand.Next(0, groupAccounts.Length));

                        var obj = "{" + $"'amount':{amount.ToString(CultureInfo.InvariantCulture)},'sender':'{sender.Id}',recipient:'{recipient.Id}', title:'{titelRnd.Next()}'" + "}";
                        userActions.Add(obj);

                        sender.Balance -= amount;
                        recipient.Balance += amount;
                    }
                    sb.Append($"[{string.Join(",", userActions)}]");
                    sb.Append("}");

                    groupActions.Add(sb.ToString());
                    sb.Clear();
                }

                groupIndex++;
            }

            return $"[{string.Join(",", groupActions)}]";
        }

        private static float GetAmount(AccountDTO[] accounts, IRnd<float> rnd)
        {
            while (true)
            {
                var amount = (float)Math.Round(rnd.Next(), 2);
                if (accounts.Any(a => a.Balance > amount))
                    return amount;
            }
        }

        private static AccountDTO GetSender(float amount, Dictionary<string, AccountDTO[]> accounts)
        {
            AccountDTO[] userAccounts;
            do
            {
                userAccounts = accounts.ElementAt(rand.Next(0, accounts.Count)).Value;
            } while (userAccounts.Any(a => a.Balance < amount));
            return GetSender(amount, userAccounts);
        }

        private static AccountDTO GetSender(float amount, AccountDTO[] accounts)
        {
            while (true)
            {
                var account = accounts[rand.Next(0, accounts.Length)];
                if (account.Balance >= amount)
                    return account;
            }
        }
    }
}

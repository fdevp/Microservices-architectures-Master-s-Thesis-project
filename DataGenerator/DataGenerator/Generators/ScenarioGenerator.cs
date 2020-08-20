using DataGenerator.DTO;
using DataGenerator.Rnd;
using Newtonsoft.Json;
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
        static ReportAggregation[] ReportAggregationValues = typeof(ReportAggregation).GetEnumValues().OfType<ReportAggregation>().ToArray();

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

            var actions = new List<IndividualUserScenarioElement>(actionsPerGroup * clients);
            foreach (var group in groups)
            {
                var groupAccounts = group.Where(g => accounts.ContainsKey(g.user.Id)).SelectMany(g => accounts[g.user.Id]).ToArray();
                var usersAccounts = groupAccounts.GroupBy(a => a.UserId).ToDictionary(k => k.Key, v => v.ToArray());

                for (int i = 0; i < actionsPerGroup; i++)
                {
                    var amount = (float)Math.Round(amountRnd.Next(), 2);
                    var sender = GetSender(amount, usersAccounts);
                    var accountId = usersAccounts[sender.UserId].OrderBy(a => Guid.NewGuid()).First().Id;
                    var card = cardsAccounts[sender.Id];
                    var user = usersDict[sender.UserId].Login;
                    var recipient = groupAccounts.ElementAt(rand.Next(0, groupAccounts.Length));
                    actions.Add(new IndividualUserScenarioElement { Group = group.Key, User = user, Amount = amount, CardId = card.Id, Recipient = recipient.Id, AccountId = accountId });

                    sender.Balance -= amount;
                    recipient.Balance += amount;
                }
            }

            return JsonConvert.SerializeObject(actions);
        }

        public static string BusinessUserScenario(SetupAll setup, int clients, int actionsPerGroup, int minTransactions, int maxTransacitons)
        {
            var users = setup.UsersSetup.Users.Where(u => u.Business)
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
            var transactionsCountRnd = new RndBuilder<int>().Min(minTransactions).Max(maxTransacitons).Build();

            var groupActions = new List<BusinessUserScenarioElement>();
            foreach (var group in groups)
            {
                var groupCount = group.Count();
                var groupAccounts = group.Where(g => accounts.ContainsKey(g.user.Id)).SelectMany(g => accounts[g.user.Id]).ToArray();
                var usersAccounts = groupAccounts.GroupBy(a => a.UserId).ToDictionary(k => k.Key, v => v.ToArray());

                for (int actions = 0; actions < actionsPerGroup; actions++)
                {
                    var user = group.ElementAt(rand.Next(0, groupCount)).user;
                    var userAccounts = usersAccounts[user.Id];
                    var transactionsCount = transactionsCountRnd.Next();
                    var userActions = new List<BusinessUserTransaction>();

                    for (int i = 0; i < transactionsCount; i++)
                    {
                        if (userAccounts.All(a => a.Balance < minAmount))
                            continue;
                        var amount = GetAmount(userAccounts, amountRnd);
                        var sender = GetSender(amount, userAccounts);
                        var recipient = groupAccounts.ElementAt(rand.Next(0, groupAccounts.Length));

                        userActions.Add(new BusinessUserTransaction { Amount = amount, AccountId = sender.Id, Recipient = recipient.Id, Title = titelRnd.Next() });

                        sender.Balance -= amount;
                        recipient.Balance += amount;
                    }

                    groupActions.Add(new BusinessUserScenarioElement { Group = group.Key, UserId = user.Id, User = user.Login, Transactions = userActions.ToArray() });
                }
            }

            return JsonConvert.SerializeObject(groupActions);
        }

        public static string UserActivityReportsScenario(SetupAll setup, int minUserReports, int maxUserReports, DateTime minDate, DateTime maxDate)
        {
            var timestampRnd = new RndBuilder<DateTime>(new DateTimeRnd()).Min(minDate).Max(maxDate).Build();
            var countRnd = new RndBuilder<int>().Min(minUserReports).Max(maxUserReports).Build();
            var granularityRnd = new RndBuilder<ReportGranularity>(new EnumRnd<ReportGranularity>()).Build();

            var actions = new List<UserActivityReportScenarioElement>();
            foreach (var user in setup.UsersSetup.Users)
            {
                var reportsCount = countRnd.Next();
                for (int i = 0; i < reportsCount; i++)
                {
                    var firstDate = timestampRnd.Next();
                    var secondDate = timestampRnd.Next();
                    var granularity = (int)granularityRnd.Next();

                    var to = firstDate > secondDate ? firstDate : secondDate;
                    var from = secondDate > firstDate ? firstDate : secondDate;

                    actions.Add(new UserActivityReportScenarioElement { UserId = user.Id, Granularity = granularity, TimestampFrom = from, TimestampTo = to });
                }
            }

            var shuffled = actions.OrderBy(elem => Guid.NewGuid()).Select(a => JsonConvert.SerializeObject(a));
            return string.Join("|", shuffled);
        }

        public static string OverallReportScenario(int min, int max, DateTime minDate, DateTime maxDate)
        {
            var timestampRnd = new RndBuilder<DateTime>(new DateTimeRnd()).Min(minDate).Max(maxDate).Build();
            var countRnd = new RndBuilder<int>().Min(min).Max(max).Build();
            var granularityRnd = new RndBuilder<ReportGranularity>(new EnumRnd<ReportGranularity>()).Build();
            var subjectRnd = new RndBuilder<ReportSubject>(new EnumRnd<ReportSubject>()).Build();
            var aggregationsCountRnd = new RndBuilder<int>().Min(1).Max(6).Build();

            var reportsCount = countRnd.Next();
            var actions = new List<OverallReportScenarioElement>();

            for (int i = 0; i < reportsCount; i++)
            {
                var firstDate = timestampRnd.Next();
                var secondDate = timestampRnd.Next();
                var granularity = (int)granularityRnd.Next();
                var subject = (int)subjectRnd.Next();
                var aggregationsCount = aggregationsCountRnd.Next();
                var aggregations = GetAggregations(aggregationsCount);

                var to = firstDate > secondDate ? firstDate : secondDate;
                var from = secondDate > firstDate ? firstDate : secondDate;

                actions.Add(new OverallReportScenarioElement { Aggregations = aggregations, Subject = subject, Granularity = granularity, TimestampFrom = from, TimestampTo = to });
            }

            var shuffled = actions.OrderBy(elem => Guid.NewGuid()).Select(a => JsonConvert.SerializeObject(a));
            return string.Join("|", shuffled);
        }

        private static int[] GetAggregations(int aggregationsCount)
        {
            var aggregations = ReportAggregationValues.OrderBy(v => Guid.NewGuid()).Take(aggregationsCount).Select(v => (int)v);
            return aggregations.ToArray();
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountsReadMicroservice.Repository;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SharedClasses;
using TransactionsReadMicroservice;
using static TransactionsReadMicroservice.TransactionsRead;

namespace AccountsReadMicroservice
{
    public class AccountsReadService : AccountsRead.AccountsReadBase
    {
        private readonly ILogger<AccountsReadService> logger;
        private readonly Mapper mapper;
        private readonly TransactionsReadClient transactionsClient;
        private readonly AccountsRepository accountsRepository;

        public AccountsReadService(AccountsRepository accountsRepository, ILogger<AccountsReadService> logger, Mapper mapper, TransactionsReadClient transactionsClient)
        {
            this.accountsRepository = accountsRepository;
            this.logger = logger;
            this.mapper = mapper;
            this.transactionsClient = transactionsClient;
        }

        public override Task<GetAccountsResponse> Get(GetAccountsRequest request, ServerCallContext context)
        {
            var accounts = request.Ids.Select(id => accountsRepository.Get(id))
                .Where(account => account != null)
                .Select(account => mapper.Map<Account>(account));
            return Task.FromResult(new GetAccountsResponse { Accounts = { accounts } });
        }

        public override Task<GetAccountsResponse> GetUserAccounts(GetUserAccountsRequest request, ServerCallContext context)
        {
            var accounts = accountsRepository.GetByUser(request.UserId).Select(account => mapper.Map<Account>(account));
            return Task.FromResult(new GetAccountsResponse { Accounts = { accounts } });
        }

        public override Task<GetBalancesResponse> GetBalances(GetBalancesRequest request, ServerCallContext context)
        {
            var balances = request.Ids.Select(id => accountsRepository.Get(id))
                .Where(account => account != null)
                .Select(account => new AccountBalance { Id = account.Id, UserId = account.UserId, Balance = account.Balance });
            return Task.FromResult(new GetBalancesResponse { Balances = { balances } });
        }

        public override async Task<AggregateUserActivityResponse> AggregateUserActivity(AggregateUserActivityRequest request, ServerCallContext context)
        {
            var accounts = accountsRepository.GetByUser(request.UserId);
            if (!accounts.Any())
                return new AggregateUserActivityResponse();

            var accountsIds = accounts.Select(a => a.Id);
            var transactionsResponse = await transactionsClient.FilterAsync(new FilterTransactionsRequest
            {
                Senders = { accountsIds },
                Recipients = { accountsIds },
                TimestampFrom = request.TimestampFrom,
                TimestampTo = request.TimestampTo
            }, context.RequestHeaders.SelectCustom());
            var transactions = transactionsResponse.Transactions.ToArray();
            var aggregated = accounts.SelectMany(a => AggregateUserTransactions(a, transactions, request.Granularity));

            return new AggregateUserActivityResponse { Portions = { aggregated } };
        }

        private IEnumerable<UserReportPortion> AggregateUserTransactions(Models.Account account, Transaction[] allTransactions, Granularity granularity)
        {
            var transactions = allTransactions.Where(t => t.Sender == account.Id || t.Recipient == account.Id);
            var withTimestamps = transactions.Select(t => new TransactionWithTimestamp { Timestamp = t.Timestamp.ToDateTime(), Transaction = t });
            var portions = Aggregations.GroupByPeriods(granularity, withTimestamps);
            var ordered = portions.OrderBy(p => p.Key);
            foreach (var portion in ordered)
            {
                var incomes = portion.Where(p => p.Transaction.Recipient == account.Id).Sum(p => (float?)p.Transaction.Amount) ?? 0;
                var debits = portion.Where(p => p.Transaction.Sender == account.Id).Sum(p => (float?)p.Transaction.Amount) ?? 0;
                yield return new UserReportPortion { Period = portion.Key, Debits = debits, Incomes = incomes, Element = account.Number };
            }
        }
    }
}

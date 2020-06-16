using System;
using System.Linq;
using System.Threading.Tasks;
using AccountsReadMicroservice.Repository;
using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
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

        public override Task<GetBalanceResponse> GetBalance(GetBalanceRequest request, ServerCallContext context)
        {
            var balances = request.Ids.Select(id => accountsRepository.Get(id))
                .Where(account => account != null)
                .Select(account => new Balance { Id = account.Id, Balance_ = account.Balance });
            return Task.FromResult(new GetBalanceResponse { Balances = { balances } });
        }

        public override async Task<GetTransactionsResponse> GetTransactions(GetTransactionsRequest request, ServerCallContext context)
        {
            var filters = new FilterTransactionsRequest { FlowId = request.FlowId, Senders = { request.Ids } };
            var response = await transactionsClient.FilterAsync(filters);
            return new GetTransactionsResponse { Transactions = { response.Transactions } };
        }
    }
}

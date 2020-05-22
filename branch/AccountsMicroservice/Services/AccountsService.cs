using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountsMicroservice.Repository;
using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using TransactionsMicroservice;
using static TransactionsMicroservice.Transactions;

namespace AccountsMicroservice
{
    public class AccountsService : Accounts.AccountsBase
    {
        private readonly ILogger<AccountsService> logger;
        private readonly Mapper mapper;
        private readonly TransactionsClient transactionsClient;
        private readonly AccountsRepository repository;

        public AccountsService(ILogger<AccountsService> logger, Mapper mapper, TransactionsClient transactionsClient)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.transactionsClient = transactionsClient;
            repository = new AccountsRepository();
        }

        public override Task<Empty> ChangeBalance(ChangeBalanceRequest request, ServerCallContext context)
        {
            repository.ChangeBalance(request.Id, request.Amount);
            return Task.FromResult(new Empty());
        }

        public override Task<GetAccountsResponse> Get(GetAccountsRequest request, ServerCallContext context)
        {
            var accounts = request.Ids.Select(id => repository.Get(id))
                .Where(account => account != null)
                .Select(account => mapper.Map<Account>(account));
            return Task.FromResult(new GetAccountsResponse { Accounts = { accounts } });
        }

        public override Task<GetBalanceResponse> GetBalance(GetBalanceRequest request, ServerCallContext context)
        {
            var balances = request.Ids.Select(id => repository.Get(id))
                .Where(account => account != null)
                .Select(account => new Balance { Id = account.Id, Balance_ = account.Balance });
            return Task.FromResult(new GetBalanceResponse { Balances = { balances } });
        }

        public override async Task<GetTransactionsResponse> GetTransactions(GetTransactionsRequest request, ServerCallContext context)
        {
            var filters = new FilterTransactionsRequest { Senders = { request.Ids } };
            var response = await transactionsClient.FilterAsync(filters);
            return new GetTransactionsResponse { Transactions = { response.Transactions } };
        }

        public override async Task<TransferResponse> Transfer(TransferRequest request, ServerCallContext context)
        {
            var account = repository.Get(request.AccountId);
            if (account == null)
                throw new ArgumentException("Account not found.");

            var recipient = repository.Get(request.Recipient);
            if (recipient == null)
                throw new ArgumentException("Recipient not found.");

            if (account.Balance < request.Amount)
                throw new ArgumentException("Not enough founds on the account.");

            var transfer = new CreateTransactionRequest
            {
                Sender = request.AccountId,
                Recipient = request.Recipient,
                Title = request.Title,
                Amount = request.Amount
            };

            var result = await transactionsClient.CreateAsync(transfer);

            repository.ChangeBalance(request.Recipient, request.Amount);
            repository.ChangeBalance(request.AccountId, request.Amount * (-1));

            return new TransferResponse { Transaction = result.Transaction };
        }

        public override Task<Empty> Setup(SetupRequest request, ServerCallContext context)
        {
            var accounts = request.Accounts.Select(a => mapper.Map<Repository.Account>(a));
            repository.Setup(accounts);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> TearDown(Empty request, ServerCallContext context)
        {
            repository.TearDown();
            return Task.FromResult(new Empty());
        }
    }
}

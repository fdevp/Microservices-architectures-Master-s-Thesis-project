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
            var filters = new FilterTransactionsRequest { FlowId = request.FlowId, Senders = { request.Ids } };
            var response = await transactionsClient.FilterAsync(filters);
            return new GetTransactionsResponse { Transactions = { response.Transactions } };
        }

        public override async Task<TransferResponse> Transfer(TransferRequest request, ServerCallContext context)
        {
            if (!repository.CanTransfer(request.AccountId, request.Recipient, request.Amount))
                throw new ArgumentException("Cannot transfer founds.");

            var transfer = CreateRequest(request);
            var result = await transactionsClient.CreateAsync(transfer);
            
            repository.Transfer(request.AccountId, request.Recipient, request.Amount);
            return new TransferResponse { Transaction = result.Transaction };
        }

        public override async Task<BatchTransferResponse> BatchTransfer(BatchTransferRequest request, Grpc.Core.ServerCallContext context)
        {
            if (request.Transfers.Any(r => !repository.CanTransfer(r.AccountId, r.Recipient, r.Amount)))
                throw new ArgumentException("Cannot transfer founds.");
            
            var transferRequests = request.Transfers.Select(r => CreateRequest(r));
            var batchAddTransactionsRequest = new BatchCreateTransactionRequest
            {
                FlowId = request.FlowId,
                Requests = { transferRequests }
            };
            var result = await transactionsClient.BatchCreateAsync(batchAddTransactionsRequest);

            foreach (var t in request.Transfers)
                repository.Transfer(t.AccountId, t.Recipient, t.Amount);
            return new BatchTransferResponse { Transactions = { { result.Transactions } } };
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

        private CreateTransactionRequest CreateRequest(TransferRequest request)
        {
            var account = repository.Get(request.AccountId);
            var recipient = repository.Get(request.Recipient);

            var transfer = new CreateTransactionRequest
            {
                FlowId = request.FlowId,
                Sender = request.AccountId,
                Recipient = request.Recipient,
                Title = request.Title,
                Amount = request.Amount
            };

            return transfer;
        }
    }
}

using System;
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
        private readonly AccountsRepository accountsRepository;

        public AccountsService(AccountsRepository accountsRepository, ILogger<AccountsService> logger, Mapper mapper, TransactionsClient transactionsClient)
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

        public override async Task<GetTransactionsResponse> GetTransactions(GetTransactionsRequest request, ServerCallContext context)
        {
            var filters = new FilterTransactionsRequest { FlowId = request.FlowId, Senders = { request.Ids }, Recipients = { request.Ids }, TimestampFrom = request.TimestampFrom, TimestampTo = request.TimestampTo };
            var response = await transactionsClient.FilterAsync(filters);
            return new GetTransactionsResponse { Transactions = { response.Transactions } };
        }

        public override async Task<TransferResponse> Transfer(TransferRequest request, ServerCallContext context)
        {
            if (!accountsRepository.CanTransfer(request.Transfer.AccountId, request.Transfer.Recipient, request.Transfer.Amount))
                throw new ArgumentException("Cannot transfer founds.");

            var transfer = CreateRequest(request.FlowId, request.Transfer);
            var result = await transactionsClient.CreateAsync(transfer);

            accountsRepository.Transfer(request.Transfer.AccountId, request.Transfer.Recipient, request.Transfer.Amount);
            return new TransferResponse { Transaction = result.Transaction };
        }

        public override async Task<Empty> BatchTransfer(BatchTransferRequest request, Grpc.Core.ServerCallContext context)
        {
            if (request.Transfers.Any(r => !accountsRepository.CanTransfer(r.AccountId, r.Recipient, r.Amount)))
                throw new ArgumentException("Cannot transfer founds.");

            var transferRequests = request.Transfers.Select(r => CreateRequest(request.FlowId, r));
            var batchAddTransactionsRequest = new BatchCreateTransactionRequest
            {
                FlowId = request.FlowId,
                Requests = { transferRequests }
            };
            var result = await transactionsClient.BatchCreateAsync(batchAddTransactionsRequest);

            foreach (var t in request.Transfers)
                accountsRepository.Transfer(t.AccountId, t.Recipient, t.Amount);
            return new Empty();
        }

        public override Task<Empty> Setup(SetupRequest request, ServerCallContext context)
        {
            var accounts = request.Accounts.Select(a => mapper.Map<Repository.Account>(a));
            accountsRepository.Setup(accounts);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> SetupAppend(SetupRequest request, ServerCallContext context)
        {
            var accounts = request.Accounts.Select(a => mapper.Map<Repository.Account>(a));
            accountsRepository.SetupAppend(accounts);
            return Task.FromResult(new Empty());
        }

        private CreateTransactionRequest CreateRequest(string flowId, Transfer request)
        {
            var account = accountsRepository.Get(request.AccountId);
            var recipient = accountsRepository.Get(request.Recipient);

            var transcation = new CreateTransactionRequest
            {
                FlowId = flowId,
                Sender = request.AccountId,
                Recipient = request.Recipient,
                Title = request.Title,
                Amount = request.Amount
            };

            return transcation;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountsWriteMicroservice.Repository;
using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SharedClasses.Messaging;
using TransactionsWriteMicroservice;
using static TransactionsWriteMicroservice.TransactionsWrite;

namespace AccountsWriteMicroservice
{
    public class AccountsWriteService : AccountsWrite.AccountsWriteBase
    {
        private readonly ILogger<AccountsWriteService> logger;
        private readonly Mapper mapper;
        private readonly TransactionsWriteClient transactionsClient;
        private readonly RabbitMqPublisher projectionChannel;
        private readonly AccountsRepository accountsRepository;

        public AccountsWriteService(AccountsRepository accountsRepository,
        ILogger<AccountsWriteService> logger,
         Mapper mapper,
          TransactionsWriteClient transactionsClient,
          RabbitMqPublisher projectionChannel)
        {
            this.accountsRepository = accountsRepository;
            this.logger = logger;
            this.mapper = mapper;
            this.transactionsClient = transactionsClient;
            this.projectionChannel = projectionChannel;
        }

        public override async Task<TransferResponse> Transfer(TransferRequest request, ServerCallContext context)
        {
            if (!accountsRepository.CanTransfer(request.Transfer.AccountId, request.Transfer.Recipient, request.Transfer.Amount))
                throw new ArgumentException("Cannot transfer founds.");

            var transfer = CreateRequest(request.FlowId, request.Transfer);
            var result = await transactionsClient.CreateAsync(transfer);

            accountsRepository.Transfer(request.Transfer.AccountId, request.Transfer.Recipient, request.Transfer.Amount);
            var affectedAccounts = new[] { accountsRepository.Get(request.Transfer.AccountId), accountsRepository.Get(request.Transfer.Recipient) };
            projectionChannel.Publish(request.FlowId.ToString(), new DataProjection<Repository.Account, string> { Upsert = affectedAccounts });

            return new TransferResponse { Transaction = result.Transaction };
        }

        public override async Task<BatchTransferResponse> BatchTransfer(BatchTransferRequest request, Grpc.Core.ServerCallContext context)
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

            var affectedAccounts = ApplyBatchToRepository(request);
            projectionChannel.Publish(request.FlowId.ToString(), new DataProjection<Repository.Account, string> { Upsert = affectedAccounts.ToArray() });

            return new BatchTransferResponse { Transactions = { { result.Transactions } } };
        }

        private IEnumerable<Repository.Account> ApplyBatchToRepository(BatchTransferRequest request)
        {
            var accounts = new Dictionary<string, Repository.Account>();
            foreach (var t in request.Transfers)
            {
                accountsRepository.Transfer(t.AccountId, t.Recipient, t.Amount);
                accounts[t.AccountId] = accountsRepository.Get(t.AccountId);
                accounts[t.Recipient] = accountsRepository.Get(t.Recipient);
            }

            return accounts.Select(a => a.Value);
        }

        private CreateTransactionRequest CreateRequest(long flowId, Transfer request)
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

        public override Task<Empty> Setup(SetupRequest request, ServerCallContext context)
        {
            var accounts = request.Accounts.Select(a => mapper.Map<Repository.Account>(a));
            accountsRepository.Setup(accounts);
            projectionChannel.Publish(null, new DataProjection<Repository.Account, string> { Upsert = accounts.ToArray() });
            return Task.FromResult(new Empty());
        }
    }
}

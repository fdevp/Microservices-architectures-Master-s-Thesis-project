using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountsWriteMicroservice.Repository;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SharedClasses;
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

            var transfer = CreateRequest(request.Transfer);
            var result = await transactionsClient.CreateAsync(transfer, context.RequestHeaders.SelectCustom());

            accountsRepository.Transfer(request.Transfer.AccountId, request.Transfer.Recipient, request.Transfer.Amount);
            var affectedAccounts = new[] { accountsRepository.Get(request.Transfer.AccountId), accountsRepository.Get(request.Transfer.Recipient) };
            if (affectedAccounts.Length > 0)
                projectionChannel.Publish(context.RequestHeaders.GetFlowId(), new DataProjection<Models.Account, string> { Upsert = affectedAccounts });

            return new TransferResponse { Transaction = result.Transaction };
        }

        public override async Task<Empty> BatchTransfer(BatchTransferRequest request, Grpc.Core.ServerCallContext context)
        {
            if (request.Transfers.Any(r => !accountsRepository.CanTransfer(r.AccountId, r.Recipient, r.Amount)))
                throw new ArgumentException("Cannot transfer founds.");

            var transferRequests = request.Transfers.Select(r => CreateRequest(r));
            var batchAddTransactionsRequest = new BatchCreateTransactionRequest
            {
                Requests = { transferRequests }
            };
            var result = await transactionsClient.BatchCreateAsync(batchAddTransactionsRequest, context.RequestHeaders.SelectCustom());

            var affectedAccounts = ApplyBatchToRepository(request).ToArray();
            if (affectedAccounts.Length > 0)
                projectionChannel.Publish(context.RequestHeaders.GetFlowId(), new DataProjection<Models.Account, string> { Upsert = affectedAccounts });

            return new Empty();
        }

        private IEnumerable<Models.Account> ApplyBatchToRepository(BatchTransferRequest request)
        {
            var accounts = new Dictionary<string, Models.Account>();
            foreach (var t in request.Transfers)
            {
                accountsRepository.Transfer(t.AccountId, t.Recipient, t.Amount);
                accounts[t.AccountId] = accountsRepository.Get(t.AccountId);
                accounts[t.Recipient] = accountsRepository.Get(t.Recipient);
            }

            return accounts.Select(a => a.Value);
        }

        private CreateTransactionRequest CreateRequest(Transfer request)
        {
            var account = accountsRepository.Get(request.AccountId);
            var recipient = accountsRepository.Get(request.Recipient);

            var transcation = new CreateTransactionRequest
            {
                Sender = request.AccountId,
                Recipient = request.Recipient,
                Title = request.Title,
                Amount = request.Amount
            };

            return transcation;
        }

        public override Task<Empty> Setup(SetupRequest request, ServerCallContext context)
        {
            var accounts = request.Accounts.Select(a => mapper.Map<Models.Account>(a));
            accountsRepository.Setup(accounts);
            projectionChannel.Publish(null, new DataProjection<Models.Account, string> { Upsert = accounts.ToArray() });
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> SetupAppend(SetupRequest request, ServerCallContext context)
        {
            var accounts = request.Accounts.Select(a => mapper.Map<Models.Account>(a));
            accountsRepository.SetupAppend(accounts);
            projectionChannel.Publish(null, new DataProjection<Models.Account, string> { Upsert = accounts.ToArray() });
            return Task.FromResult(new Empty());
        }
    }
}

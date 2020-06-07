using System;
using System.Linq;
using System.Threading.Tasks;
using AccountsWriteMicroservice.Repository;
using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using TransactionsWriteMicroservice;
using static TransactionsWriteMicroservice.TransactionsWrite;

namespace AccountsWriteMicroservice
{
    public class AccountsWriteService : AccountsWrite.AccountsWriteBase
    {
        private readonly ILogger<AccountsWriteService> logger;
        private readonly Mapper mapper;
        private readonly TransactionsWriteClient transactionsClient;
        private readonly AccountsRepository accountsRepository;

        public AccountsWriteService(AccountsRepository accountsRepository,ILogger<AccountsWriteService> logger, Mapper mapper, TransactionsWriteClient transactionsClient)
        {
            this.accountsRepository = accountsRepository;
            this.logger = logger;
            this.mapper = mapper;
            this.transactionsClient = transactionsClient;
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

            foreach (var t in request.Transfers)
                accountsRepository.Transfer(t.AccountId, t.Recipient, t.Amount);
            return new BatchTransferResponse { Transactions = { { result.Transactions } } };
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
    }
}

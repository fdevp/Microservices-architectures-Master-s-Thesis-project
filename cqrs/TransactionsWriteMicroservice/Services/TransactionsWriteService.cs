using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using TransactionsWriteMicroservice.Repository;

namespace TransactionsWriteMicroservice
{
    public class TransactionsWriteService : TransactionsWrite.TransactionsWriteBase
    {
        private readonly ILogger<TransactionsWriteService> logger;
        private readonly Mapper mapper;
        private TransactionsRepository transactionsRepository;

        public TransactionsWriteService(TransactionsRepository transactionsRepository, ILogger<TransactionsWriteService> logger, Mapper mapper)
        {
            this.transactionsRepository = transactionsRepository;
            this.logger = logger;
            this.mapper = mapper;
        }
        public override Task<CreateTransactionResult> Create(CreateTransactionRequest request, ServerCallContext context)
        {
            var transaction = transactionsRepository.Create(request.Title, request.Amount, request.Recipient, request.Sender, request.PaymentId, request.CardId);
            return Task.FromResult(new CreateTransactionResult { Transaction = mapper.Map<Transaction>(transaction) });
        }

        public override Task<BatchCreateTransactionResult> BatchCreate(BatchCreateTransactionRequest request, ServerCallContext context)
        {
            var transactions = request.Requests
                .Select(r => transactionsRepository.Create(r.Title, r.Amount, r.Recipient, r.Sender, r.PaymentId, r.CardId))
                .Select(transaction => mapper.Map<Transaction>(transaction))
                .ToArray();
            return Task.FromResult(new BatchCreateTransactionResult { Transactions = { transactions } });
        }
    }
}

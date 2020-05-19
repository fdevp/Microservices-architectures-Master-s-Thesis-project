using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using TransactionsMicroservice.Repository;

namespace TransactionsMicroservice
{
    public class TransactionsService : Transactions.TransactionsBase
    {
        private readonly ILogger<TransactionsService> _logger;
        private readonly Mapper mapper;
        private TransactionsRepository _repository;

        public TransactionsService(ILogger<TransactionsService> logger, Mapper mapper)
        {
            _logger = logger;
            this.mapper = mapper;
        }

        public override async Task<GetTransactionsResult> Get(GetTransactionsRequest request, ServerCallContext context)
        {
            var transactions = request.Ids.Select(id => _repository.Get(id))
                .Where(transaction => transaction != null)
                .Select(transaction => mapper.Map<Transaction>(transaction))
                .ToArray();
            return new GetTransactionsResult { Transactions = { transactions } };
        }

        public override async Task<CreateTransactionResult> Create(CreateTransactionRequest request, ServerCallContext context)
        {
            var transaction = _repository.Create(request.Title, request.Amount, request.Recipient, request.Sender, request.PaymentId, request.CardId);
            return new CreateTransactionResult { Transaction = mapper.Map<Transaction>(transaction) };
        }

        public override async Task<BatchCreateTransactionResult> BatchCreate(BatchCreateTransactionRequest request, ServerCallContext context)
        {
            var transactions = request.Requests
                .Select(r => _repository.Create(r.Title, r.Amount, r.Recipient, r.Sender, r.PaymentId, r.CardId))
                .Select(transaction => mapper.Map<Transaction>(transaction))
                .ToArray();
            return new BatchCreateTransactionResult { Transactions = { transactions } };
        }
    }
}

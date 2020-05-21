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
        private TransactionsRepository _repository = new TransactionsRepository();

        public TransactionsService(ILogger<TransactionsService> logger, Mapper mapper)
        {
            _logger = logger;
            this.mapper = mapper;
        }

        public override Task<GetTransactionsResult> Get(GetTransactionsRequest request, ServerCallContext context)
        {
            var transactions = request.Ids.Select(id => _repository.Get(id))
                .Where(transaction => transaction != null)
                .Select(transaction => mapper.Map<Transaction>(transaction))
                .ToArray();
            return Task.FromResult(new GetTransactionsResult { Transactions = { transactions } });
        }

        public override Task<CreateTransactionResult> Create(CreateTransactionRequest request, ServerCallContext context)
        {
            var transaction = _repository.Create(request.Title, request.Amount, request.Recipient, request.Sender, request.PaymentId, request.CardId);
            return Task.FromResult(new CreateTransactionResult { Transaction = mapper.Map<Transaction>(transaction) });
        }

        public override Task<BatchCreateTransactionResult> BatchCreate(BatchCreateTransactionRequest request, ServerCallContext context)
        {
            var transactions = request.Requests
                .Select(r => _repository.Create(r.Title, r.Amount, r.Recipient, r.Sender, r.PaymentId, r.CardId))
                .Select(transaction => mapper.Map<Transaction>(transaction))
                .ToArray();
            return Task.FromResult(new BatchCreateTransactionResult { Transactions = { transactions } });
        }

        public override Task<GetTransactionsResult> Filter(FilterTransactionsRequest request, ServerCallContext context)
        {
            var filters = new Filters
            {
                Cards = request.Cards.ToHashSet(),
                Payments = request.Payments.ToHashSet(),
                Recipients = request.Recipients.ToHashSet(),
                Senders = request.Senders.ToHashSet(),
                TimestampFrom = request.TimestampFrom,
                TimestampTo = request.TimestampTo,
            };

            var transactions = _repository.GetMany(filters).Select(t => mapper.Map<Transaction>(t));
            return Task.FromResult(new GetTransactionsResult { Transactions = { transactions } });
        }
    }
}

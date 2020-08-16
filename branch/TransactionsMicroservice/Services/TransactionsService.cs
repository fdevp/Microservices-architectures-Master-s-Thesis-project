using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using TransactionsMicroservice.Repository;

namespace TransactionsMicroservice
{
    public class TransactionsService : Transactions.TransactionsBase
    {
        private readonly ILogger<TransactionsService> logger;
        private readonly Mapper mapper;
        private TransactionsRepository transactionsRepository;

        public TransactionsService(TransactionsRepository transactionsRepository, ILogger<TransactionsService> logger, Mapper mapper)
        {
            this.transactionsRepository = transactionsRepository;
            this.logger = logger;
            this.mapper = mapper;
        }

        public override Task<GetTransactionsResult> Get(GetTransactionsRequest request, ServerCallContext context)
        {
            var transactions = request.Ids.Select(id => transactionsRepository.Get(id))
                .Where(transaction => transaction != null)
                .Select(transaction => mapper.Map<Transaction>(transaction))
                .ToArray();
            return Task.FromResult(new GetTransactionsResult { Transactions = { transactions } });
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

            var transactions = transactionsRepository.GetMany(filters, request.Top).Select(t => mapper.Map<Transaction>(t));
            return Task.FromResult(new GetTransactionsResult { Transactions = { transactions } });
        }

        public override Task<Empty> Setup(SetupRequest request, ServerCallContext context)
        {
            var transactions = request.Transactions.Select(t => mapper.Map<Repository.Transaction>(t));
            transactionsRepository.Setup(transactions);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> SetupAppend(SetupRequest request, ServerCallContext context)
        {
            var transactions = request.Transactions.Select(t => mapper.Map<Repository.Transaction>(t));
            transactionsRepository.SetupAppend(transactions);
            return Task.FromResult(new Empty());
        }
    }
}

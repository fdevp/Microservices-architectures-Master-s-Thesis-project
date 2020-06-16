using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using TransactionsReadMicroservice.Repository;

namespace TransactionsReadMicroservice
{
    public class TransactionsReadService : TransactionsRead.TransactionsReadBase
    {
        private readonly ILogger<TransactionsReadService> logger;
        private readonly Mapper mapper;
        private TransactionsRepository transactionsRepository;

        public TransactionsReadService(TransactionsRepository transactionsRepository, ILogger<TransactionsReadService> logger, Mapper mapper)
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

            var transactions = transactionsRepository.GetMany(filters).Select(t => mapper.Map<Transaction>(t));
            return Task.FromResult(new GetTransactionsResult { Transactions = { transactions } });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SharedClasses;
using SharedClasses.Messaging;
using TransactionsWriteMicroservice.Repository;

namespace TransactionsWriteMicroservice
{
    public class TransactionsWriteService : TransactionsWrite.TransactionsWriteBase
    {
        private readonly ILogger<TransactionsWriteService> logger;
        private readonly Mapper mapper;
        private readonly RabbitMqPublisher projectionChannel;
        private TransactionsRepository transactionsRepository;

        public TransactionsWriteService(TransactionsRepository transactionsRepository, ILogger<TransactionsWriteService> logger, Mapper mapper, RabbitMqPublisher projectionChannel)
        {
            this.transactionsRepository = transactionsRepository;
            this.logger = logger;
            this.mapper = mapper;
            this.projectionChannel = projectionChannel;
        }
        public override Task<CreateTransactionResult> Create(CreateTransactionRequest request, ServerCallContext context)
        {
            var transaction = transactionsRepository.Create(request.Title, request.Amount, request.Recipient, request.Sender, request.PaymentId, request.CardId);
            projectionChannel.Publish(context.RequestHeaders.GetFlowId(), new DataProjection<Models.Transaction, string> { Upsert = new[] { transaction } });
            return Task.FromResult(new CreateTransactionResult { Transaction = mapper.Map<Transaction>(transaction) });
        }

        public override Task<Empty> BatchCreate(BatchCreateTransactionRequest request, ServerCallContext context)
        {
            var transactions = request.Requests.Select(r => transactionsRepository.Create(r.Title, r.Amount, r.Recipient, r.Sender, r.PaymentId, r.CardId)).ToArray();
            if (transactions.Length > 0)
                projectionChannel.Publish(context.RequestHeaders.GetFlowId(), new DataProjection<Models.Transaction, string> { Upsert = transactions });
            var response = transactions.Select(transaction => mapper.Map<Transaction>(transaction))
                .ToArray();
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> Setup(SetupRequest request, ServerCallContext context)
        {
            var transactions = request.Transactions.Select(t => mapper.Map<Models.Transaction>(t));
            transactionsRepository.Setup(transactions);
            projectionChannel.Publish(null, new DataProjection<Models.Transaction, string> { Upsert = transactions.ToArray() });
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> SetupAppend(SetupRequest request, ServerCallContext context)
        {
            var transactions = request.Transactions.Select(t => mapper.Map<Models.Transaction>(t));
            transactionsRepository.SetupAppend(transactions);
            projectionChannel.Publish(null, new DataProjection<Models.Transaction, string> { Upsert = transactions.ToArray() });
            return Task.FromResult(new Empty());
        }
    }
}

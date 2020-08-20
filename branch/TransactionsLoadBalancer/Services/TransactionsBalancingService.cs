using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SharedClasses;
using TransactionsMicroservice;
using static TransactionsMicroservice.Transactions;

namespace TransactionsLoadBalancer
{
    public class TransactionsBalancingService : Transactions.TransactionsBase
    {
        private readonly ILogger<TransactionsBalancingService> _logger;
        private readonly TransactionsClient[] services;

        public TransactionsBalancingService(ILogger<TransactionsBalancingService> logger, IEnumerable<TransactionsClient> services)
        {
            _logger = logger;
            this.services = services.ToArray();
        }

        public override async Task<GetTransactionsResult> Get(GetTransactionsRequest request, ServerCallContext context)
        {
            var groupedIds = request.Ids.GroupBy(id => GetServiceIndex(id)).ToArray();
            var tasks = groupedIds.Select(g => services[g.Key].GetAsync(new GetTransactionsRequest { Ids = { g } }, context.RequestHeaders.SelectCustom()).ResponseAsync);

            var results = await Task.WhenAll(tasks);
            var transactions = results.SelectMany(r => r.Transactions);
            return new GetTransactionsResult { Transactions = { transactions } };
        }

        public override async Task<CreateTransactionResult> Create(CreateTransactionRequest request, ServerCallContext context)
        {
            var service = services[GetServiceIndex(context.RequestHeaders.GetFlowId())];
            var result = await service.CreateAsync(request, context.RequestHeaders.SelectCustom());
            return result;
        }

        public override async Task<BatchCreateTransactionResult> BatchCreate(BatchCreateTransactionRequest request, ServerCallContext context)
        {
            var groupedRequests = request.Requests.GroupBy(r => GetServiceIndex(Guid.NewGuid()));
            var tasks = groupedRequests.Select(g => services[g.Key].BatchCreateAsync(new BatchCreateTransactionRequest { Requests = { g } }, context.RequestHeaders.SelectCustom()).ResponseAsync);
            var results = await Task.WhenAll(tasks);
            return new BatchCreateTransactionResult { Transactions = { results.SelectMany(r => r.Transactions) } };
        }

        public override async Task<GetTransactionsResult> Filter(FilterTransactionsRequest request, ServerCallContext context)
        {
            var tasks = services.Select(s => s.FilterAsync(request, context.RequestHeaders.SelectCustom()).ResponseAsync);
            var results = await Task.WhenAll(tasks);
            return new GetTransactionsResult { Transactions = { results.SelectMany(r => r.Transactions) } };
        }

        public override async Task<Empty> Setup(SetupRequest request, ServerCallContext context)
        {
            var grouped = request.Transactions.GroupBy(t => GetServiceIndex(t.Id)).ToArray();
            var tasks = grouped.Select(g => services[g.Key].SetupAsync(new SetupRequest { Transactions = { g } }).ResponseAsync);
            await Task.WhenAll(tasks);
            return new Empty();
        }

        public override async Task<Empty> SetupAppend(SetupRequest request, ServerCallContext context)
        {
            var grouped = request.Transactions.GroupBy(t => GetServiceIndex(t.Id)).ToArray();
            var tasks = grouped.Select(g => services[g.Key].SetupAppendAsync(new SetupRequest { Transactions = { g } }).ResponseAsync);
            await Task.WhenAll(tasks);
            return new Empty();
        }

        private int GetServiceIndex(string guid) => GetServiceIndex(new Guid(guid));
        private int GetServiceIndex(Guid guid) => Math.Abs(guid.GetHashCode()) % services.Length;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SharedClasses;
using TransactionsReadMicroservice;
using static TransactionsReadMicroservice.TransactionsRead;

namespace TransactionsLoadBalancer
{
    public class TransactionsReadBalancingService : TransactionsRead.TransactionsReadBase
    {
        private readonly ILogger<TransactionsReadBalancingService> _logger;
        private readonly TransactionsReadClient[] services;

        public TransactionsReadBalancingService(ILogger<TransactionsReadBalancingService> logger, IEnumerable<TransactionsReadClient> readServices)
        {
            _logger = logger;
            this.services = readServices.ToArray();
        }

        public override async Task<GetTransactionsResult> Get(GetTransactionsRequest request, ServerCallContext context)
        {
            var groupedIds = request.Ids.GroupBy(id => GetServiceIndex(id)).ToArray();
            var tasks = groupedIds.Select(g => services[g.Key].GetAsync(new GetTransactionsRequest { Ids = { g } }, context.RequestHeaders.SelectCustom()).ResponseAsync);

            var results = await Task.WhenAll(tasks);
            var transactions = results.SelectMany(r => r.Transactions);
            return new GetTransactionsResult { Transactions = { transactions } };
        }

        public override async Task<AggregateOverallResponse> AggregateOverall(AggregateOverallRequest request, ServerCallContext context)
        {
            var tasks = services.Select(s => s.AggregateOverallAsync(request, context.RequestHeaders.SelectCustom()).ResponseAsync);
            var results = await Task.WhenAll(tasks);
            return new AggregateOverallResponse { Portions = { results.SelectMany(r => r.Portions) } };
        }

        public override async Task<GetTransactionsResult> Filter(FilterTransactionsRequest request, ServerCallContext context)
        {
            var tasks = services.Select(s => s.FilterAsync(request, context.RequestHeaders.SelectCustom()).ResponseAsync);
            var results = await Task.WhenAll(tasks);
            return new GetTransactionsResult { Transactions = { results.SelectMany(r => r.Transactions) } };
        }

        private int GetServiceIndex(string guid) => GetServiceIndex(new Guid(guid));
        private int GetServiceIndex(Guid guid) => Math.Abs(guid.GetHashCode()) % services.Length;
    }
}

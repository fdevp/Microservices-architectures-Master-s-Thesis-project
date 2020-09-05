using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SharedClasses;
using TransactionsWriteMicroservice;
using static TransactionsWriteMicroservice.TransactionsWrite;

namespace TransactionsLoadBalancer
{
    public class TransactionsWriteBalancingService : TransactionsWrite.TransactionsWriteBase
    {
        private readonly ILogger<TransactionsWriteBalancingService> _logger;
        private readonly TransactionsWriteClient[] services;

        public TransactionsWriteBalancingService(ILogger<TransactionsWriteBalancingService> logger, IEnumerable<TransactionsWriteClient> writeServices)
        {
            _logger = logger;
            this.services = writeServices.ToArray();
        }

        public override async Task<CreateTransactionResult> Create(CreateTransactionRequest request, ServerCallContext context)
        {
            var service = services[GetServiceIndex(context.RequestHeaders.GetFlowId())];
            var result = await service.CreateAsync(request);
            return result;
        }

        public override async Task<Empty> BatchCreate(BatchCreateTransactionRequest request, ServerCallContext context)
        {
            var groupedRequests = request.Requests.GroupBy(r => GetServiceIndex(Guid.NewGuid()));
            var tasks = groupedRequests.Select(g => services[g.Key].BatchCreateAsync(new BatchCreateTransactionRequest { Requests = { g } }, context.RequestHeaders.SelectCustom()).ResponseAsync);
            var results = await Task.WhenAll(tasks);
            return new Empty();
        }

        public override async Task<Empty> Setup(SetupRequest request, ServerCallContext context)
        {
            if (request.Transactions.Count == 0)
            {
                var tasks = services.Select(s => s.SetupAsync(new SetupRequest()).ResponseAsync);
                await Task.WhenAll(tasks);
            }
            else
            {
                var grouped = request.Transactions.GroupBy(t => GetServiceIndex(t.Id)).ToArray();
                var tasks = grouped.Select(g => services[g.Key].SetupAsync(new SetupRequest { Transactions = { g } }).ResponseAsync);
                await Task.WhenAll(tasks);
            }
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountsMicroservice;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static AccountsMicroservice.Accounts;

namespace AccountsLoadBalancer
{
    public class AccountsBalancingService : Accounts.AccountsBase
    {
        private readonly ILogger<AccountsBalancingService> _logger;
        private readonly AccountsClient[] services;

        public AccountsBalancingService(ILogger<AccountsBalancingService> logger, IEnumerable<AccountsClient> services)
        {
            _logger = logger;
            this.services = services.ToArray();
        }

        public override async Task<GetAccountsResponse> Get(GetAccountsRequest request, ServerCallContext context)
        {
            var groupedIds = request.Ids.GroupBy(id => GetServiceIndex(id)).ToArray();
            var tasks = groupedIds.Select(g => services[g.Key].GetAsync(new GetAccountsRequest { FlowId = request.FlowId, Ids = { g } }).ResponseAsync);

            var results = await Task.WhenAll(tasks);
            var accounts = results.SelectMany(r => r.Accounts);
            return new GetAccountsResponse { Accounts = { accounts } };
        }

        public override async Task<GetAccountsResponse> GetUserAccounts(GetUserAccountsRequest request, ServerCallContext context)
        {
            var tasks = services.Select(s => s.GetUserAccountsAsync(request).ResponseAsync);
            var results = await Task.WhenAll(tasks);
            var accounts = results.SelectMany(r => r.Accounts);
            return new GetAccountsResponse { Accounts = { accounts } };
        }

        public override async Task<GetBalancesResponse> GetBalances(GetBalancesRequest request, ServerCallContext context)
        {
            var groupedIds = request.Ids.GroupBy(id => GetServiceIndex(id)).ToArray();
            var tasks = groupedIds.Select(g => services[g.Key].GetBalancesAsync(new GetBalancesRequest { FlowId = request.FlowId, Ids = { g } }).ResponseAsync);

            var results = await Task.WhenAll(tasks);
            var balances = results.SelectMany(r => r.Balances);
            return new GetBalancesResponse { Balances = { balances } };
        }

        public override async Task<GetTransactionsResponse> GetTransactions(GetTransactionsRequest request, ServerCallContext context)
        {
            var groupedIds = request.Ids.GroupBy(id => GetServiceIndex(id)).ToArray();
            var tasks = groupedIds.Select(g => services[g.Key].GetTransactionsAsync(new GetTransactionsRequest { FlowId = request.FlowId, Ids = { g } }).ResponseAsync);
            var results = await Task.WhenAll(tasks);
            var transactions = results.SelectMany(r => r.Transactions);
            return new GetTransactionsResponse { Transactions = { transactions } };
        }

        public override async Task<TransferResponse> Transfer(TransferRequest request, ServerCallContext context)
        {
            var service = services[GetServiceIndex(request.FlowId)];
            var result = await service.TransferAsync(request);
            return result;
        }

        public override async Task<BatchTransferResponse> BatchTransfer(BatchTransferRequest request, Grpc.Core.ServerCallContext context)
        {
            var grouped = request.Transfers.GroupBy(t => GetServiceIndex(t.AccountId)).ToArray();
            var tasks = grouped.Select(g => services[g.Key].BatchTransferAsync(new BatchTransferRequest { FlowId = request.FlowId, Transfers = { g } }).ResponseAsync);
            await Task.WhenAll(tasks);
            return new BatchTransferResponse(); //TODO
        }

        public override async Task<Empty> Setup(SetupRequest request, ServerCallContext context)
        {
            var grouped = request.Accounts.GroupBy(t => GetServiceIndex(t.Id)).ToArray();
            var tasks = grouped.Select(g => services[g.Key].SetupAsync(new SetupRequest { Accounts = { g } }).ResponseAsync);
            await Task.WhenAll(tasks);
            return new Empty();
        }

        private int GetServiceIndex(string guid) => GetServiceIndex(new Guid(guid));
        private int GetServiceIndex(Guid guid) => Math.Abs(guid.GetHashCode()) % services.Length;
    }
}

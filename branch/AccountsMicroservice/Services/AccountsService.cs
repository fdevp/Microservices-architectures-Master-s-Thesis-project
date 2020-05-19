using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountsMicroservice.Repository;
using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace AccountsMicroservice
{
    public class AccountsService : Accounts.AccountsBase
    {
        private readonly ILogger<AccountsService> _logger;
        private readonly Mapper mapper;
        private readonly AccountsRepository _repository;

        public AccountsService(ILogger<AccountsService> logger, Mapper mapper)
        {
            _logger = logger;
            this.mapper = mapper;
            _repository = new AccountsRepository();
        }

        public override async Task<Empty> ChangeBalance(ChangeBalanceRequest request, ServerCallContext context)
        {
            _repository.ChangeBalance(request.Id, request.Amount);
            return new Empty();
        }

        public override async Task<GetAccountsResponse> Get(GetAccountsRequest request, ServerCallContext context)
        {
            var accounts = request.Ids.Select(id => _repository.Get(id))
                .Where(account => account != null)
                .Select(account => mapper.Map<Account>(account));
            return new GetAccountsResponse { Accounts = { accounts } };
        }

        public override async Task<GetBalanceResponse> GetBalance(GetBalanceRequest request, ServerCallContext context)
        {
            var balances = request.Ids.Select(id => _repository.Get(id))
                .Where(account => account != null)
                .Select(account => new Balance { Id = account.Id, Balance_ = account.Balance });
            return new GetBalanceResponse { Balances = { balances } };
        }

        public override async Task<GetTransactionsResponse> GetTransactions(GetTransactionsRequest request, ServerCallContext context)
        {

        }

        public override async Task<TransferResponse> Transfer(TransferRequest request, ServerCallContext context)
        {

        }
    }
}

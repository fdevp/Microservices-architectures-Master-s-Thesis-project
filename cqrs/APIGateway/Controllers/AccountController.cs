using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountsReadMicroservice;
using AccountsWriteMicroservice;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static AccountsReadMicroservice.AccountsRead;
using static AccountsWriteMicroservice.AccountsWrite;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> logger;
        private readonly AccountsWriteClient accountsWriteClient;
        private readonly AccountsReadClient accountsReadClient;
        private readonly Mapper mapper;

        public AccountController(ILogger<AccountController> logger, AccountsWriteClient accountsWriteClient, AccountsReadClient accountsReadClient, Mapper mapper)
        {
            this.logger = logger;
            this.accountsWriteClient = accountsWriteClient;
            this.accountsReadClient = accountsReadClient;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<IEnumerable<AccountDTO>> Get(string userId)
        {
            var request = new GetUserAccountsRequest { FlowId = HttpContext.Items["flowId"].ToString(), UserId = userId };
            var response = await accountsReadClient.GetUserAccountsAsync(request);
            var accounts = response.Accounts.Select(a => mapper.Map<AccountDTO>(a));
            return accounts;
        }

        [HttpGet]
        [Route("balance/{accountId}")]
        public async Task<float> Balance(string accountId)
        {
            var request = new GetBalancesRequest { FlowId = HttpContext.Items["flowId"].ToString(), Ids = { accountId } };
            var repsonse = await accountsReadClient.GetBalancesAsync(request);
            var balance = repsonse.Balances.Single();
            return balance.Balance;
        }

        [HttpGet]
        [Route("balances")]
        public async Task<BalanceDTO[]> Balance(string[] ids)
        {
            var request = new GetBalancesRequest { FlowId = HttpContext.Items["flowId"].ToString(), Ids = { ids } };
            var response = await accountsReadClient.GetBalancesAsync(request);
            return response.Balances.Select(b => mapper.Map<BalanceDTO>(b)).ToArray();
        }

        [HttpPost]
        [Route("transfer")]
        public async Task Transfer(AccountTransfer data)
        {
            var transfer = mapper.Map<Transfer>(data);
            var request = new TransferRequest { FlowId = HttpContext.Items["flowId"].ToString(), Transfer = transfer };
            await accountsWriteClient.TransferAsync(request);
        }

        [HttpPost]
        [Route("setup")]
        public async Task Setup(AccountsSetup setup)
        {
            for (int i = 0; i < setup.Accounts.Length; i += 10000)
            {
                var portion = setup.Accounts.Skip(i).Take(10000).ToArray();
                var request = mapper.Map<SetupRequest>(new AccountsSetup { Accounts = portion });
                await accountsWriteClient.SetupAppendAsync(request);
            }
        }
    }
}
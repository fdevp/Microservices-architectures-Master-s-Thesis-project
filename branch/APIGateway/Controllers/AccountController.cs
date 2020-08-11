using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountsMicroservice;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static AccountsMicroservice.Accounts;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> logger;
        private readonly AccountsClient accountClient;
        private readonly Mapper mapper;

        public AccountController(ILogger<AccountController> logger, AccountsClient accountClient, Mapper mapper)
        {
            this.logger = logger;
            this.accountClient = accountClient;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<IEnumerable<AccountDTO>> Get(string userId)
        {
            var request = new GetUserAccountsRequest { UserId = userId };
            request.FlowId = HttpContext.Items["flowId"].ToString();
            var response = await accountClient.GetUserAccountsAsync(request);
            var accounts = response.Accounts.Select(a => mapper.Map<AccountDTO>(a));
            return accounts;
        }

        [HttpGet]
        [Route("balance/{accountId}")]
        public async Task<float> Balance(string accountId)
        {
            var request = new GetBalancesRequest { Ids = { accountId } };
            request.FlowId = HttpContext.Items["flowId"].ToString();
            var repsonse = await accountClient.GetBalancesAsync(request);
            var balance = repsonse.Balances.Single();
            return balance.Balance;
        }

        [HttpPost]
        [Route("transfer")]
        public async Task Transfer(AccountTransfer data)
        {
            var transfer = mapper.Map<Transfer>(data);
            var request = new TransferRequest { Transfer = transfer };
            request.FlowId = HttpContext.Items["flowId"].ToString();
            await accountClient.TransferAsync(request);
        }

        [HttpPost]
        [Route("setup")]
        public async Task Setup(AccountsSetup setup)
        {
            for (int i = 0; i < setup.Accounts.Length; i += 10000)
            {
                var portion = setup.Accounts.Skip(i).Take(10000).ToArray();
                var request = mapper.Map<SetupRequest>(new AccountsSetup { Accounts = portion });
                await accountClient.SetupAppendAsync(request);
            }
        }
    }
}
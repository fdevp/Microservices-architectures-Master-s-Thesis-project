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
            request.FlowId = (long)HttpContext.Items["flowId"];
            var response = await accountClient.GetUserAccountsAsync(request);
            var accounts = response.Accounts.Select(a => mapper.Map<AccountDTO>(a));
            return accounts;
        }

        [HttpGet]
        [Route("balance/{accountId}")]
        public async Task<float> Balance(string accountId)
        {
            var request = new GetBalancesRequest { Ids = { accountId } };
            request.FlowId = (long)HttpContext.Items["flowId"];
            var repsonse = await accountClient.GetBalancesAsync(request);
            var balance = repsonse.Balances.Single();
            return balance.Balance;
        }

        [HttpPost]
        [Route("transfer")]
        public async Task<TransactionDTO> Transfer(AccountTransfer data)
        {
            var transfer = mapper.Map<Transfer>(data);
            var request = new TransferRequest { Transfer = transfer };
            request.FlowId = (long)HttpContext.Items["flowId"];
            var response = await accountClient.TransferAsync(request);
            var transcation = mapper.Map<TransactionDTO>(response.Transaction);
            return transcation;
        }

        [HttpPost]
        [Route("setup")]
        public async Task Setup(AccountsSetup setup)
        {
            var request = mapper.Map<SetupRequest>(setup);
            await accountClient.SetupAsync(request);
        }
    }
}
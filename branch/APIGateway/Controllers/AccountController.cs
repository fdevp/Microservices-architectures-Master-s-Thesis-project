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
        public IEnumerable<AccountDTO> Get()
        {
            return new AccountDTO[0];
        }

        [HttpGet]
        [Route("balance")]
        public async Task<float> Balance(string accountId)
        {
            var request = new GetBalanceRequest { Ids = { accountId } };
            var repsonse = await accountClient.GetBalanceAsync(request);
            var balance = repsonse.Balances.Single();
            return balance.Balance_;
        }

        [HttpPost]
        [Route("transfer")]
        public async Task<TransactionDTO> Transfer(AccountTransfer data)
        {
            var request = mapper.Map<TransferRequest>(data);
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

        [HttpPost]
        [Route("teardown")]
        public async Task TearDown()
        {
            await accountClient.TearDownAsync(new Empty());
        }

    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedClasses.Events;
using SharedClasses.Events.Accounts;
using SharedClasses.Events.Transactions;
using SharedClasses.Messaging;
using SharedClasses.Models;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> logger;
        private readonly PublishingRouter publishingRouter;
        private readonly EventsAwaiter eventsAwaiter;
        private readonly Mapper mapper;

        public AccountController(ILogger<AccountController> logger, PublishingRouter publishingRouter, EventsAwaiter eventsAwaiter, Mapper mapper)
        {
            this.logger = logger;
            this.publishingRouter = publishingRouter;
            this.eventsAwaiter = eventsAwaiter;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<IEnumerable<AccountDTO>> Get(string userId)
        {
            var flowId = HttpContext.Items["flowId"].ToString();
            var payload = new GetUserAccountsEvent { UserId = userId };
            var response = await eventsAwaiter.AwaitResponse<SelectedAccountsEvent>(flowId, () => publishingRouter.Publish(Queues.Accounts, payload, flowId, Queues.APIGateway));
            var accounts = response.Accounts.Select(a => mapper.Map<AccountDTO>(a));
            return accounts;
        }

        [HttpGet]
        [Route("balance/{accountId}")]
        public async Task<float> Balance(string accountId)
        {
            var flowId = HttpContext.Items["flowId"].ToString();
            var payload = new GetBalancesEvent { Ids = new[] { accountId } };
            var response = await eventsAwaiter.AwaitResponse<SelectedBalancesEvent>(flowId, () => publishingRouter.Publish(Queues.Accounts, payload, flowId, Queues.APIGateway));
            var balance = response.Balances.Single();
            return balance.Balance;
        }

        [HttpGet]
        [Route("balances")]
        public async Task<BalanceDTO[]> Balance(string[] ids)
        {
            var flowId = HttpContext.Items["flowId"].ToString();
            var payload = new GetBalancesEvent { Ids = ids };
            var response = await eventsAwaiter.AwaitResponse<SelectedBalancesEvent>(flowId, () => publishingRouter.Publish(Queues.Accounts, payload, flowId, Queues.APIGateway));
            return response.Balances.Select(b => mapper.Map<BalanceDTO>(b)).ToArray();
        }

        [HttpPost]
        [Route("transfer")]
        public Task Transfer(Transfer transfer)
        {
            var flowId = HttpContext.Items["flowId"].ToString();
            var payload = new TransferEvent { Transfer = transfer };
            publishingRouter.Publish(Queues.Accounts, payload, flowId);
            return Task.CompletedTask;
        }

        [HttpPost]
        [Route("setup")]
        public Task Setup(AccountsSetup setup)
        {
            var payload = mapper.Map<SetupAccountsEvent>(setup);
            publishingRouter.Publish(Queues.Accounts, payload, null);
            return Task.CompletedTask;
        }
    }
}
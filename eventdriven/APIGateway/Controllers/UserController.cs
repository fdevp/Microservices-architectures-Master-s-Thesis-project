using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedClasses.Events.Accounts;
using SharedClasses.Events.Cards;
using SharedClasses.Events.Transactions;
using SharedClasses.Events.Users;
using SharedClasses.Messaging;
using SharedClasses.Models;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> logger;
        private readonly PublishingRouter publishingRouter;
        private readonly EventsAwaiter eventsAwaiter;
        private readonly Mapper mapper;

        public UserController(ILogger<UserController> logger,
         PublishingRouter publishingRouter,
         EventsAwaiter eventsAwaiter,
         Mapper mapper)
        {
            this.logger = logger;
            this.publishingRouter = publishingRouter;
            this.eventsAwaiter = eventsAwaiter;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("{userId}/panel")]
        public async Task<Panel> Panel(string userId)
        {
            var mainFlowId = HttpContext.Items["flowId"].ToString();

            var panel = new Panel();

            var accountsFlowId = mainFlowId + "_a";
            var accountsEvent = new GetUserAccountsEvent { UserId = userId };
            var accountsResponse = await eventsAwaiter.AwaitResponse<SelectedAccountsEvent>(accountsFlowId, () => publishingRouter.Publish(Queues.Accounts, accountsEvent, accountsFlowId, Queues.APIGateway));
            panel.Accounts = mapper.Map<AccountDTO[]>(accountsResponse.Accounts);
            var accountsIds = accountsResponse.Accounts.Select(a => a.Id).ToArray();

            var parallelTasks = new List<Task>();
            parallelTasks.Add(Task.Run(async () =>
            {
                var transactionsFlowId = mainFlowId + "_t";
                var transactionsEvent = new FilterTransactionsEvent { Senders = accountsIds };
                var transactionsResponse = await eventsAwaiter.AwaitResponse<SelectedTransactionsEvent>(transactionsFlowId, () => publishingRouter.Publish(Queues.Transactions, accountsEvent, transactionsFlowId, Queues.APIGateway));
                panel.Transactions = mapper.Map<TransactionDTO[]>(transactionsResponse.Transactions);
            }));

            // parallelTasks.Add(Task.Run(async () =>
            // {
            //     var paymentsAndLoans = await paymentsReadClient.GetWithLoansAsync(new GetPaymentsWithLoansRequest { FlowId = flowId, AccountIds = { accountsIds } });
            //     loans = paymentsAndLoans.Loans;
            //     payments = paymentsAndLoans.Payments;
            // }));

            parallelTasks.Add(Task.Run(async () =>
            {
                var cardsFlowId = mainFlowId + "_c";
                var cardsEvent = new GetCardsByAccountsEvent { AccountsIds = accountsIds };
                var cardsResponse = await eventsAwaiter.AwaitResponse<SelectedCardsEvent>(cardsFlowId, () => publishingRouter.Publish(Queues.Cards, accountsEvent, cardsFlowId, Queues.APIGateway));
                panel.Cards = mapper.Map<CardDTO[]>(cardsResponse.Cards);
            }));


            await Task.WhenAll(parallelTasks);
            return panel;
        }

        [HttpPost]
        [Route("token")]
        public async Task<string> Token(TokenRequest data)
        {
            var flowId = HttpContext.Items["flowId"].ToString();
            var tokenEvent = new CreateTokenEvent { Login = data.Login, Password = data.Password };
            var response = await eventsAwaiter.AwaitResponse<NewTokenEvent>(flowId, () => publishingRouter.Publish(Queues.Users, tokenEvent, flowId, Queues.APIGateway));
            return response.Token;
        }

        [HttpPost]
        [Route("logout")]
        public Task Logout(LogoutRequest data)
        {
            var flowId = HttpContext.Items["flowId"].ToString();
            var logoutEvent = new LogoutEvent { Token = data.Token };
            publishingRouter.Publish(Queues.Users, logoutEvent, flowId);
            return Task.CompletedTask;
        }

        [HttpPost]
        [Route("messages")]
        public Task Messages(BatchMessages data)
        {
            var flowId = HttpContext.Items["flowId"].ToString();
            var messages = data.Messages.Select(m => mapper.Map<UserMessage>(m)).ToArray();
            var batchMessagesEvent = new BatchAddMessagesEvent { Messages = messages };
            publishingRouter.Publish(Queues.Users, batchMessagesEvent, flowId);
            return Task.CompletedTask;
        }

        [HttpPost]
        [Route("setup")]
        public Task Setup(UsersSetup setup)
        {
            var payload = mapper.Map<SetupUsersEvent>(setup);
            publishingRouter.Publish(Queues.Users, payload, null);
            return Task.CompletedTask;
        }
    }
}
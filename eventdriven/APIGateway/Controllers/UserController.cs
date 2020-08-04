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
using SharedClasses.Events.Loans;
using SharedClasses.Events.Payments;
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
        private const int PanelTransactionsCount = 5;
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

            var panel = new Panel { Accounts = new AccountDTO[0], Cards = new CardDTO[0], Payments = new PaymentDTO[0], Transactions = new TransactionDTO[0], Loans = new LoanDTO[0] };

            var accountsFlowId = mainFlowId + "_a";
            var accountsEvent = new GetUserAccountsEvent { UserId = userId };
            var accountsResponse = await eventsAwaiter.AwaitResponse<SelectedAccountsEvent>(accountsFlowId, () => publishingRouter.Publish(Queues.Accounts, accountsEvent, accountsFlowId, Queues.APIGateway));
            panel.Accounts = mapper.Map<AccountDTO[]>(accountsResponse.Accounts);
            var accountsIds = accountsResponse.Accounts.Select(a => a.Id).ToArray();

            if (accountsIds.Any())
            {
                var parallelTasks = new List<Task>();
                parallelTasks.Add(Task.Run(async () =>
                {
                    var transactionsFlowId = mainFlowId + "_t";
                    var transactionsEvent = new FilterTransactionsEvent { Senders = accountsIds, Top = PanelTransactionsCount };
                    var transactionsResponse = await eventsAwaiter.AwaitResponse<SelectedTransactionsEvent>(transactionsFlowId, () => publishingRouter.Publish(Queues.Transactions, transactionsEvent, transactionsFlowId, Queues.APIGateway));
                    panel.Transactions = mapper.Map<TransactionDTO[]>(transactionsResponse.Transactions);
                }));

                parallelTasks.Add(Task.Run(async () =>
                {
                    var paymentsFlowId = mainFlowId + "_p";
                    var paymentsEvent = new GetPaymentsByAccountsEvent { AccountsIds = accountsIds };
                    var paymentsResponse = await eventsAwaiter.AwaitResponse<SelectedPaymentsEvent>(paymentsFlowId, () => publishingRouter.Publish(Queues.Payments, paymentsEvent, paymentsFlowId, Queues.APIGateway));
                    panel.Payments = mapper.Map<PaymentDTO[]>(paymentsResponse.Payments);

                    var loansFlowId = mainFlowId + "_l";
                    var paymentsIds = paymentsResponse.Payments.Select(p => p.Id).ToArray();
                    var loansEvent = new GetLoansByPaymentsEvent { PaymentsIds = paymentsIds };
                    var loansResponse = await eventsAwaiter.AwaitResponse<SelectedLoansEvent>(loansFlowId, () => publishingRouter.Publish(Queues.Loans, loansEvent, loansFlowId, Queues.APIGateway));
                    panel.Loans = mapper.Map<LoanDTO[]>(loansResponse.Loans);
                }));

                parallelTasks.Add(Task.Run(async () =>
                {
                    var cardsFlowId = mainFlowId + "_c";
                    var cardsEvent = new GetCardsByAccountsEvent { AccountsIds = accountsIds };
                    var cardsResponse = await eventsAwaiter.AwaitResponse<SelectedCardsEvent>(cardsFlowId, () => publishingRouter.Publish(Queues.Cards, cardsEvent, cardsFlowId, Queues.APIGateway));
                    panel.Cards = mapper.Map<CardDTO[]>(cardsResponse.Cards);
                }));
                
                await Task.WhenAll(parallelTasks);
            }

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
        [Route("setup")]
        public Task Setup(UsersSetup setup)
        {
            var payload = mapper.Map<SetupUsersEvent>(setup);
            publishingRouter.Publish(Queues.Users, payload, null);
            return Task.CompletedTask;
        }
    }
}
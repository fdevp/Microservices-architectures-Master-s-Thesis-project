using System.Threading.Tasks;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SharedClasses.Events.Accounts;
using SharedClasses.Events.Cards;
using SharedClasses.Events.Loans;
using SharedClasses.Events.Payments;
using SharedClasses.Events.Transactions;
using SharedClasses.Events.Users;
using SharedClasses.Messaging;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SetupController : ControllerBase
    {
        private readonly PublishingRouter publishingRouter;
        private readonly Mapper mapper;

        public SetupController(PublishingRouter publishingRouter,
         Mapper mapper)
        {
            this.publishingRouter = publishingRouter;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("setup")]
        public Task Setup(SetupAll setup)
        {
            var usersEvent = mapper.Map<SetupUsersEvent>(setup.UsersSetup);
            this.publishingRouter.Publish(Queues.Users, usersEvent, null);

            var accountsEvent = mapper.Map<SetupAccountsEvent>(setup.AccountsSetup);
            this.publishingRouter.Publish(Queues.Accounts, accountsEvent, null);

            var cardsEvent = mapper.Map<SetupCardsEvent>(setup.CardsSetup);
            this.publishingRouter.Publish(Queues.Cards, cardsEvent, null);

            var loansEvent = mapper.Map<SetupLoansEvent>(setup.LoansSetup);
            this.publishingRouter.Publish(Queues.Loans, loansEvent, null);

            var paymentsEvent = mapper.Map<SetupPaymentsEvent>(setup.PaymentsSetup);
            this.publishingRouter.Publish(Queues.Payments, paymentsEvent, null);

            var transactionsEvent = mapper.Map<SetupTransactionsEvent>(setup.TransactionsSetup);
            this.publishingRouter.Publish(Queues.Transactions, transactionsEvent, null);

            return Task.CompletedTask;
        }
    }
}

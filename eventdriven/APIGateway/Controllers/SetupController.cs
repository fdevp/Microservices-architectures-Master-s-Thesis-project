using System.Threading.Tasks;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SharedClasses.Events.Accounts;
using SharedClasses.Events.Transactions;
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
            //var usersRequest = mapper.Map<UsersMicroservice.SetupRequest>(setup.UsersSetup);
            var accountsEvent = mapper.Map<SetupAccountsEvent>(setup.AccountsSetup);
            this.publishingRouter.Publish(Queues.Accounts, accountsEvent, null);
            // var cardsRequest = mapper.Map<CardsWriteMicroservice.SetupRequest>(setup.CardsSetup);
            // var loansRequest = mapper.Map<LoansWriteMicroservice.SetupRequest>(setup.LoansSetup);
            // var paymentsRequest = mapper.Map<PaymentsWriteMicroservice.SetupRequest>(setup.PaymentsSetup);
            var transactionsEvent = mapper.Map<SetupTransactionsEvent>(setup.TransactionsSetup);
            this.publishingRouter.Publish(Queues.Transactions, transactionsEvent, null);

            return Task.CompletedTask;
        }
    }
}

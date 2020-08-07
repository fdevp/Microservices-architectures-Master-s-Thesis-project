using System.Linq;
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
using SharedClasses.Models;

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
        [Route("clear")]
        public Task Clear()
        {
            this.publishingRouter.Publish(Queues.Users, new SetupUsersEvent(), null);
            this.publishingRouter.Publish(Queues.Accounts, new SetupAccountsEvent(), null);
            this.publishingRouter.Publish(Queues.Cards, new SetupCardsEvent(), null);
            this.publishingRouter.Publish(Queues.Loans, new SetupLoansEvent(), null);
            this.publishingRouter.Publish(Queues.Payments, new SetupPaymentsEvent(), null);
            return Task.CompletedTask;
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
            
            for (int i = 0; i < setup.LoansSetup.loans.Length; i += 10000)
            {
                var portion = setup.LoansSetup.loans.Skip(i).Take(10000).ToArray();
                var loansEvent = new SetupAppendLoansEvent { Loans = portion.Select(l => mapper.Map<Loan>(l)).ToArray() };
                this.publishingRouter.Publish(Queues.Loans, loansEvent, null);
            }

            for (int i = 0; i < setup.PaymentsSetup.payments.Length; i += 10000)
            {
                var portion = setup.PaymentsSetup.payments.Skip(i).Take(10000).ToArray();
                var paymentsEvent = new SetupAppendPaymentsEvent { Payments = portion.Select(p => mapper.Map<Payment>(p)).ToArray() };
                this.publishingRouter.Publish(Queues.Payments, paymentsEvent, null);
            }

            for (int i = 0; i < setup.TransactionsSetup.transactions.Length; i += 10000)
            {
                var portion = setup.TransactionsSetup.transactions.Skip(i).Take(10000).ToArray();
                var transactionsEvent = new SetupAppendTransactionsEvent { Transactions = portion.Select(t => mapper.Map<Transaction>(t)).ToArray() };
                this.publishingRouter.Publish(Queues.Transactions, transactionsEvent, null);
            }

            return Task.CompletedTask;
        }
    }
}

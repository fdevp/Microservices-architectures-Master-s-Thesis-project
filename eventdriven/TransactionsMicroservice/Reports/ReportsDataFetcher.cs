using System;
using System.Linq;
using System.Threading.Tasks;
using SharedClasses.Events;
using SharedClasses.Events.Accounts;
using SharedClasses.Events.Cards;
using SharedClasses.Events.Loans;
using SharedClasses.Events.Payments;
using SharedClasses.Events.Transactions;
using SharedClasses.Messaging;
using SharedClasses.Messaging.RabbitMq;
using SharedClasses.Models;

namespace TransactionsMicroservice.Reports
{
    public class ReportsDataFetcher
    {
        private readonly PublishingRouter publishingRouter;
        private readonly EventsAwaiter eventsAwaiter;
        private readonly RabbitMqConfig config;

        public ReportsDataFetcher(PublishingRouter publishingRouter, EventsAwaiter eventsAwaiter, RabbitMqConfig config)
        {
            this.publishingRouter = publishingRouter;
            this.eventsAwaiter = eventsAwaiter;
            this.config = config;
        }

        public async Task<Account[]> GetAccounts(string flowId, string userId)
        {
            var payload = new GetUserAccountsEvent { UserId = userId };
            var response = await eventsAwaiter.AwaitResponse<SelectedAccountsEvent>(flowId, () => publishingRouter.Publish(Queues.Accounts, payload, flowId + "_a", config.Queue));
            return response.Accounts;
        }

        public async Task<Payment[]> GetPayments(string flowId, string[] accountsIds)
        {
            var payload = new GetPaymentsByAccountsEvent { AccountsIds = accountsIds };
            var response = await eventsAwaiter.AwaitResponse<SelectedPaymentsEvent>(flowId, () => publishingRouter.Publish(Queues.Payments, payload, flowId + "_p", config.Queue));
            return response.Payments;
        }

        public async Task<Loan[]> GetLoans(string flowId, string[] accountsIds)
        {
            var payload = new GetLoansByAccountsEvent { AccountsIds = accountsIds };
            var response = await eventsAwaiter.AwaitResponse<SelectedLoansEvent>(flowId, () => publishingRouter.Publish(Queues.Loans, payload, flowId + "_l", config.Queue));
            return response.Loans;
        }

        public async Task<Card[]> GetCards(string flowId, string[] accountsIds)
        {
            var payload = new GetCardsByAccountsEvent { AccountsIds = accountsIds };
            var response = await eventsAwaiter.AwaitResponse<SelectedCardsEvent>(flowId, () => publishingRouter.Publish(Queues.Cards, payload, flowId + "_c", config.Queue));
            return response.Cards;
        }
    }
}
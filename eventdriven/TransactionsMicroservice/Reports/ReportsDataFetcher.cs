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
            var portionFlowId = flowId + "_a";
            var payload = new GetUserAccountsEvent { UserId = userId };
            var response = await eventsAwaiter.AwaitResponse<SelectedAccountsEvent>(portionFlowId, () => publishingRouter.Publish(Queues.Accounts, payload, portionFlowId, config.Queue));
            return response.Accounts;
        }

        public async Task<Payment[]> GetPayments(string flowId, string[] accountsIds)
        {
            var portionFlowId = flowId + "_p";
            var payload = new GetPaymentsByAccountsEvent { AccountsIds = accountsIds };
            var response = await eventsAwaiter.AwaitResponse<SelectedPaymentsEvent>(portionFlowId, () => publishingRouter.Publish(Queues.Payments, payload, portionFlowId, config.Queue));
            return response.Payments;
        }

        public async Task<Loan[]> GetLoans(string flowId, string[] accountsIds)
        {
            var portionFlowId = flowId + "_l";
            var payload = new GetLoansByAccountsEvent { AccountsIds = accountsIds };
            var response = await eventsAwaiter.AwaitResponse<SelectedLoansEvent>(portionFlowId, () => publishingRouter.Publish(Queues.Loans, payload, portionFlowId, config.Queue));
            return response.Loans;
        }

        public async Task<Card[]> GetCards(string flowId, string[] accountsIds)
        {
            var portionFlowId = flowId + "_c";
            var payload = new GetCardsByAccountsEvent { AccountsIds = accountsIds };
            var response = await eventsAwaiter.AwaitResponse<SelectedCardsEvent>(portionFlowId, () => publishingRouter.Publish(Queues.Cards, payload, portionFlowId, config.Queue));
            return response.Cards;
        }
    }
}
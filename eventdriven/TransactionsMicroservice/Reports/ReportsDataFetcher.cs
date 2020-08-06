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
using SharedClasses.Models;

namespace TransactionsMicroservice.Reports
{
    public class ReportsDataFetcher
    {
        private readonly PublishingRouter publishingRouter;
        private readonly EventsAwaiter eventsAwaiter;

        public ReportsDataFetcher(PublishingRouter publishingRouter, EventsAwaiter eventsAwaiter)
        {
            this.publishingRouter = publishingRouter;
            this.eventsAwaiter = eventsAwaiter;
        }

        public async Task<Account[]> GetAccounts(string flowId, string userId)
        {
            var payload = new GetUserAccountsEvent { UserId = userId };
            var response = await eventsAwaiter.AwaitResponse<SelectedAccountsEvent>(flowId, () => publishingRouter.Publish(Queues.Accounts, payload, flowId, Queues.Transactions));
            return response.Accounts;
        }

        public async Task<Payment[]> GetPayments(string flowId, string[] accountsIds)
        {
            var payload = new GetPaymentsByAccountsEvent { AccountsIds = accountsIds };
            var response = await eventsAwaiter.AwaitResponse<SelectedPaymentsEvent>(flowId, () => publishingRouter.Publish(Queues.Payments, payload, flowId, Queues.Transactions));
            return response.Payments;
        }

        public async Task<Loan[]> GetLoans(string flowId, string[] accountsIds)
        {
            var payload = new GetLoansByAccountsEvent { AccountsIds = accountsIds };
            var response = await eventsAwaiter.AwaitResponse<SelectedLoansEvent>(flowId, () => publishingRouter.Publish(Queues.Loans, payload, flowId, Queues.Transactions));
            return response.Loans;
        }

        public async Task<Card[]> GetCards(string flowId, string[] accountsIds)
        {
            var payload = new GetCardsByAccountsEvent { AccountsIds = accountsIds };
            var response = await eventsAwaiter.AwaitResponse<SelectedCardsEvent>(flowId, () => publishingRouter.Publish(Queues.Cards, payload, flowId, Queues.Transactions));
            return response.Cards; ;
        }

        public async Task<Transaction[]> GetTransactions(string flowId, string queue, DateTime? from, DateTime? to)
        {
            var payload = new GetTransactionsEvent { TimestampFrom = from, TimestampTo = to };
            var response = await eventsAwaiter.AwaitResponse<SelectedTransactionsEvent>(flowId, () => publishingRouter.Publish(queue, payload, flowId, Queues.Transactions));
            return response.Transactions;
        }

        public async Task<Transaction[]> GetTransactions(string flowId, FilterTransactionsEvent payload)
        {
            var response = await eventsAwaiter.AwaitResponse<SelectedTransactionsEvent>(flowId, () => publishingRouter.Publish(Queues.Transactions, payload, flowId, Queues.Transactions));
            return response.Transactions;
        }
    }
}
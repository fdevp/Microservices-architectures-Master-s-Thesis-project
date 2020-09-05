using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharedClasses.Balancing;
using SharedClasses.Events;
using SharedClasses.Events.Reports;
using SharedClasses.Events.Transactions;
using SharedClasses.Messaging;

namespace TransactionsLoadBalancer
{
    public class TransactionsBalancingService
    {
        private readonly BalancingQueues balancingQueues;
        private readonly ILogger<TransactionsBalancingService> logger;
        private readonly PublishingRouter publishingRouter;
        private readonly EventsAwaiter eventsAwaiter;

        public TransactionsBalancingService(BalancingQueues balancingQueues, PublishingRouter publishingRouter, EventsAwaiter eventsAwaiter, ILogger<TransactionsBalancingService> logger)
        {
            this.balancingQueues = balancingQueues;
            this.logger = logger;
            this.publishingRouter = publishingRouter;
            this.eventsAwaiter = eventsAwaiter;
        }

        [EventHandlingMethod(typeof(GetTransactionsEvent))]
        public async Task Get(MessageContext context, GetTransactionsEvent inputEvent)
        {
            var groupedIds = inputEvent.Ids.GroupBy(id => GetQueue(id)).ToArray();
            var tasks = groupedIds.Select(g =>
            {
                var queue = g.Key;
                var portionFlowId = GetFlowId(context.FlowId, queue);
                var portionEvent = new GetTransactionsEvent { Ids = g.ToArray(), TimestampFrom = inputEvent.TimestampFrom, TimestampTo = inputEvent.TimestampTo };
                return eventsAwaiter.AwaitResponse<SelectedTransactionsEvent>(portionFlowId, () => publishingRouter.Publish(queue, portionEvent, portionFlowId, Queues.Transactions));
            });

            var results = await Task.WhenAll(tasks);
            var transactions = results.SelectMany(r => r.Transactions);
            publishingRouter.Publish(context.ReplyTo, new SelectedTransactionsEvent { Transactions = transactions.ToArray() }, context.FlowId);
        }

        [EventHandlingMethod(typeof(CreateTransactionEvent))]
        public Task Create(MessageContext context, CreateTransactionEvent inputEvent)
        {
            var queue = GetQueue(context.FlowId);
            publishingRouter.Publish(context.ReplyTo, inputEvent, context.FlowId);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(BatchCreateTransactionEvent))]
        public Task BatchCreate(MessageContext context, BatchCreateTransactionEvent inputEvent)
        {
            var groupedByQueues = inputEvent.Requests.GroupBy(id => GetQueue(Guid.NewGuid())).ToArray();
            foreach (var group in groupedByQueues)
            {
                var queue = group.Key;
                var portionEvent = new BatchCreateTransactionEvent { Requests = group.ToArray() };
                publishingRouter.Publish(queue, portionEvent, context.FlowId);
            }
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(FilterTransactionsEvent))]
        public async Task Filter(MessageContext context, FilterTransactionsEvent inputEvent)
        {
            var tasks = balancingQueues.Queues.Select(queue =>
            {
                var portionFlowId = GetFlowId(context.FlowId, queue);
                return eventsAwaiter.AwaitResponse<SelectedTransactionsEvent>(portionFlowId, () => publishingRouter.Publish(queue, inputEvent, portionFlowId, Queues.Transactions));
            });

            var results = await Task.WhenAll(tasks);
            var transactions = results.SelectMany(r => r.Transactions);
            publishingRouter.Publish(context.ReplyTo, new SelectedTransactionsEvent { Transactions = transactions.ToArray() }, context.FlowId);
        }

        [EventHandlingMethod(typeof(AggregateOverallReportDataEvent))]
        public async Task GenerateOverallReport(MessageContext context, AggregateOverallReportDataEvent inputEvent)
        {
            var tasks = balancingQueues.Queues.Select(queue =>
            {
                var portionFlowId = GetFlowId(context.FlowId, queue);
                return eventsAwaiter.AwaitResponse<AggregatedOverallReportEvent>(portionFlowId, () => publishingRouter.Publish(queue, inputEvent, portionFlowId, Queues.Transactions));
            });

            var results = await Task.WhenAll(tasks);
            var portions = results.SelectMany(r => r.Portions);
            publishingRouter.Publish(context.ReplyTo, new AggregatedOverallReportEvent { Portions = portions.ToArray() }, context.FlowId);
        }

        [EventHandlingMethod(typeof(AggregateUserActivityReportDataEvent))]
        public async Task GenerateUserActivityReport(MessageContext context, AggregateUserActivityReportDataEvent inputEvent)
        {
            var tasks = balancingQueues.Queues.Select(queue =>
            {
                var portionFlowId = GetFlowId(context.FlowId, queue);
                return eventsAwaiter.AwaitResponse<AggregatedUserActivityReportEvent>(portionFlowId, () => publishingRouter.Publish(queue, inputEvent, portionFlowId, Queues.Transactions));
            });

            var results = await Task.WhenAll(tasks);
            var accountsPortions = results.SelectMany(r => r.AccountsPortions).ToArray();
            var cardsPortions = results.SelectMany(r => r.CardsPortions).ToArray();
            var loansPortions = results.SelectMany(r => r.LoansPortions).ToArray();
            var paymentsPortions = results.SelectMany(r => r.PaymentsPortions).ToArray();

            var response = new AggregatedUserActivityReportEvent { PaymentsPortions = paymentsPortions, LoansPortions = loansPortions, CardsPortions = cardsPortions, AccountsPortions = accountsPortions };
            publishingRouter.Publish(context.ReplyTo, response, context.FlowId);
        }

        [EventHandlingMethod(typeof(SetupTransactionsEvent))]
        public Task Setup(MessageContext context, SetupTransactionsEvent inputEvent)
        {
            if (inputEvent.Transactions.Length == 0)
            {
                foreach (var queue in balancingQueues.Queues)
                {
                    publishingRouter.Publish(queue, new SetupTransactionsEvent { Transactions = new SharedClasses.Models.Transaction[0] }, context.FlowId);
                }
            }
            else
            {
                var groupedIds = inputEvent.Transactions.GroupBy(transaction => GetQueue(transaction.Id)).ToArray();
                foreach (var group in groupedIds)
                {
                    var queue = group.Key;
                    var portionEvent = new SetupTransactionsEvent { Transactions = group.ToArray() };
                    publishingRouter.Publish(queue, portionEvent, context.FlowId);
                }
            }

            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(SetupAppendTransactionsEvent))]
        public Task SetupAppend(MessageContext context, SetupAppendTransactionsEvent inputEvent)
        {
            var groupedIds = inputEvent.Transactions.GroupBy(transaction => GetQueue(transaction.Id)).ToArray();
            foreach (var group in groupedIds)
            {
                var queue = group.Key;
                var portionEvent = new SetupAppendTransactionsEvent { Transactions = group.ToArray() };
                publishingRouter.Publish(queue, portionEvent, context.FlowId);
            }

            return Task.CompletedTask;
        }

        private string GetFlowId(string flowId, string queueName) => $"{flowId}_{queueName}";
        private string GetQueue(string guid) => GetQueue(new Guid(guid));
        private string GetQueue(Guid guid) => balancingQueues.Queues[Math.Abs(guid.GetHashCode()) % balancingQueues.Queues.Length];
    }
}
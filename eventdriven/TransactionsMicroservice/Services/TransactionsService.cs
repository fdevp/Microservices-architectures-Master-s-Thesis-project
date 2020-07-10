using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharedClasses.Events;
using SharedClasses.Events.Transactions;
using SharedClasses.Messaging;
using TransactionsMicroservice.Repository;

namespace TransactionsMicroservice
{
    public class TransactionsService
    {
        private readonly ILogger<TransactionsService> logger;
        private readonly PublishingRouter publishingRouter;
        private TransactionsRepository transactionsRepository;

        public TransactionsService(TransactionsRepository transactionsRepository, ILogger<TransactionsService> logger, PublishingRouter publishingRouter)
        {
            this.transactionsRepository = transactionsRepository;
            this.logger = logger;
            this.publishingRouter = publishingRouter;
        }

        [EventHandlingMethod(typeof(GetTransactionsEvent))]
        public Task Get(MessageContext context, GetTransactionsEvent inputEvent)
        {
            var transactions = inputEvent.Ids.Select(id => transactionsRepository.Get(id))
                .Where(transaction => transaction != null)
                .ToArray();
            publishingRouter.Publish(context.ReplyTo, new SelectedTransactionsEvent { Transactions = transactions }, context.FlowId);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(CreateTransactionEvent))]
        public Task Create(MessageContext context, CreateTransactionEvent inputEvent)
        {
            var transaction = transactionsRepository.Create(inputEvent.Title, inputEvent.Amount, inputEvent.Recipient, inputEvent.Sender, inputEvent.PaymentId, inputEvent.CardId);
            
            if (context.ReplyTo != null)
                publishingRouter.Publish(context.ReplyTo, new SelectedTransactionsEvent { Transactions = new[] { transaction } }, context.FlowId);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(BatchCreateTransactionEvent))]
        public Task BatchCreate(MessageContext context, BatchCreateTransactionEvent inputEvent)
        {
            var transactions = inputEvent.Requests
                .Select(r => transactionsRepository.Create(r.Title, r.Amount, r.Recipient, r.Sender, r.PaymentId, r.CardId))
                .ToArray();

            if (context.ReplyTo != null)
                publishingRouter.Publish(context.ReplyTo, new SelectedTransactionsEvent { Transactions = transactions }, context.FlowId);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(FilterTransactionsEvent))]
        public Task Filter(MessageContext context, FilterTransactionsEvent inputEvent)
        {
            var filters = new Filters
            {
                Cards = inputEvent.Cards?.ToHashSet(),
                Payments = inputEvent.Payments?.ToHashSet(),
                Recipients = inputEvent.Recipients?.ToHashSet(),
                Senders = inputEvent.Senders.ToHashSet(),
                TimestampFrom = inputEvent.TimestampFrom,
                TimestampTo = inputEvent.TimestampTo,
            };

            var transactions = transactionsRepository.GetMany(filters, inputEvent.Top);
            publishingRouter.Publish(context.ReplyTo, new SelectedTransactionsEvent { Transactions = transactions }, context.FlowId);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(SetupTransactionsEvent))]
        public Task Setup(MessageContext context, SetupTransactionsEvent inputEvent)
        {
            transactionsRepository.Setup(inputEvent.Transactions);
            return Task.CompletedTask;
        }
    }
}


using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PaymentsMicroservice.Repository;
using SharedClasses.Events;
using SharedClasses.Events.Payments;
using SharedClasses.Events.Transactions;
using SharedClasses.Messaging;

namespace PaymentsMicroservice
{
    public class PaymentsService
    {
        private readonly ILogger<PaymentsService> logger;
        private readonly PublishingRouter publishingRouter;
        private readonly PaymentsRepository paymentsRepository;

        public PaymentsService(PaymentsRepository paymentsRepository, ILogger<PaymentsService> logger, PublishingRouter publishingRouter)
        {
            this.paymentsRepository = paymentsRepository;
            this.logger = logger;
            this.publishingRouter = publishingRouter;
        }

        [EventHandlingMethod(typeof(GetPaymentsEvent))]
        public Task Get(MessageContext context, GetPaymentsEvent inputEvent)
        {
            var payments = inputEvent.Ids.Select(id => paymentsRepository.Get(id))
                .Where(payment => payment != null)
                .ToArray();
            publishingRouter.Publish(context.ReplyTo, new SelectedPaymentsEvent { Payments = payments }, context.FlowId);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(CreatePaymentEvent))]
        public Task Create(MessageContext context, CreatePaymentEvent inputEvent)
        {
            var payment = paymentsRepository.Create(inputEvent.Amount, inputEvent.StartTimestamp, inputEvent.Interval, inputEvent.AccountId, inputEvent.Recipient);
            if (context.ReplyTo != null)
                publishingRouter.Publish(context.ReplyTo, new SelectedPaymentsEvent { Payments = new[] { payment } }, context.FlowId);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(CancelPaymentsEvent))]
        public Task Cancel(MessageContext context, CancelPaymentsEvent inputEvent)
        {
            foreach (var id in inputEvent.Ids)
                paymentsRepository.Cancel(id);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(GetTransactionsEvent))]
        public Task GetTransactions(MessageContext context, GetTransactionsEvent inputEvent)
        {
            var paymentsTransactionsEvent = new FilterTransactionsEvent { Payments = inputEvent.Ids };
            publishingRouter.Publish(context.ReplyTo, paymentsTransactionsEvent, context.FlowId);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(SetupPaymentsEvent))]
        public Task Setup(MessageContext context, SetupPaymentsEvent inputEvent)
        {
            paymentsRepository.Setup(inputEvent.Payments);
            return Task.CompletedTask;
        }
    }
}

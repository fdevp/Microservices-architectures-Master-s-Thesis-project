
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoansMicroservice.Repository;
using Microsoft.Extensions.Logging;
using SharedClasses.Events.Loans;
using SharedClasses.Events.Payments;
using SharedClasses.Messaging;

namespace LoansMicroservice
{
    public class LoansService
    {
        private readonly ILogger<LoansService> logger;
        private readonly PublishingRouter publishingRouter;
        private LoansRepository loansRepository;

        public LoansService(LoansRepository loansRepository, ILogger<LoansService> logger, PublishingRouter publishingRouter)
        {
            this.loansRepository = loansRepository;
            this.logger = logger;
            this.publishingRouter = publishingRouter;
        }


        [EventHandlingMethod(typeof(GetLoansEvent))]
        public Task Get(MessageContext context, GetLoansEvent inputEvent)
        {
            var loans = inputEvent.Ids.Select(id => loansRepository.Get(id))
                .Where(loan => loan != null).ToArray();
            publishingRouter.Publish(context.ReplyTo, new SelectedLoansEvent { Loans = loans }, context.FlowId);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(GetLoansByPaymentsEvent))]
        public Task GetLoansByPayments(MessageContext context, GetLoansByPaymentsEvent inputEvent)
        {
            var loans = loansRepository.GetByPayment(inputEvent.PaymentsIds);
            publishingRouter.Publish(context.ReplyTo, new SelectedLoansEvent { Loans = loans }, context.FlowId);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(BatchRepayInstalmentsEvent))]
        public Task BatchRepayInstalments(MessageContext context, BatchRepayInstalmentsEvent inputEvent)
        {
            var paymentsToFinish = RepayInstalments(inputEvent);
            var cancelPaymentsEvent = new CancelPaymentsEvent { Ids = paymentsToFinish.ToArray() };
            publishingRouter.Publish(Queues.Payments, cancelPaymentsEvent, context.FlowId);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(SetupLoansEvent))]
        public Task Setup(MessageContext context, SetupLoansEvent inputEvent)
        {
            loansRepository.Setup(inputEvent.Loans);
            return Task.CompletedTask;
        }

        private IEnumerable<string> RepayInstalments(BatchRepayInstalmentsEvent inputEvent)
        {
            foreach (var id in inputEvent.Ids)
            {
                var totalAmountPaid = loansRepository.RepayInstalment(id);
                if (totalAmountPaid)
                    yield return loansRepository.Get(id).PaymentId;
            }
        }
    }
}

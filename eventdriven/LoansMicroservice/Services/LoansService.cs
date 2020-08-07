
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoansMicroservice.Repository;
using Microsoft.Extensions.Logging;
using SharedClasses.Events;
using SharedClasses.Events.Loans;
using SharedClasses.Events.Payments;
using SharedClasses.Events.Transactions;
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
            var loans = loansRepository.GetByPayments(inputEvent.PaymentsIds);
            publishingRouter.Publish(context.ReplyTo, new SelectedLoansEvent { Loans = loans }, context.FlowId);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(GetLoansByAccountsEvent))]
        public Task GetLoansByAccounts(MessageContext context, GetLoansByAccountsEvent inputEvent)
        {
            var loans = loansRepository.GetByAccounts(inputEvent.AccountsIds);
            publishingRouter.Publish(context.ReplyTo, new SelectedLoansEvent { Loans = loans }, context.FlowId);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(GetTransactionsEvent))]
        public Task GetTransactions(MessageContext context, GetTransactionsEvent inputEvent)
        {
            var paymentsIds = inputEvent.Ids != null && inputEvent.Ids.Length > 0 ? inputEvent.Ids : loansRepository.GetPaymentsIds();
            var getTransactionsEvent = new FilterTransactionsEvent { Payments = paymentsIds, TimestampFrom = inputEvent.TimestampFrom, TimestampTo = inputEvent.TimestampTo };
            publishingRouter.Publish(Queues.Transactions, getTransactionsEvent, context.FlowId, context.ReplyTo);
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

        [EventHandlingMethod(typeof(SetupAppendLoansEvent))]
        public Task SetupAppend(MessageContext context, SetupLoansEvent inputEvent)
        {
            loansRepository.SetupAppend(inputEvent.Loans);
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

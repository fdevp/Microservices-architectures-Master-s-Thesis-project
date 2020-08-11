using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APIGateway.Models;
using AutoMapper;
using Google.Protobuf.Collections;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedClasses.Events.Accounts;
using SharedClasses.Events.Loans;
using SharedClasses.Events.Payments;
using SharedClasses.Events.Users;
using SharedClasses.Messaging;
using SharedClasses.Models;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BatchController : ControllerBase
    {
        private readonly ILogger<BatchController> logger;
        private readonly Mapper mapper;
        private readonly PublishingRouter publishingRouter;
        private readonly EventsAwaiter eventsAwaiter;

        public BatchController(ILogger<BatchController> logger, Mapper mapper,
         PublishingRouter publishingRouter,
         EventsAwaiter eventsAwaiter)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.publishingRouter = publishingRouter;
            this.eventsAwaiter = eventsAwaiter;
        }

        [HttpGet]
        public async Task<BatchData> Get([FromQuery(Name = "part")] int part, [FromQuery(Name = "total")] int total)
        {
            var flowId = HttpContext.Items["flowId"].ToString();

            Payment[] payments = null;
            Loan[] loans = null;
            AccountBalance[] balances = null;

            var paymentsFlowId = flowId + "_p";
            var paymentsEvent = new GetPartPaymentsEvent { Part = part, TotalParts = total };
            var paymentsResponse = await eventsAwaiter.AwaitResponse<SelectedPaymentsEvent>(paymentsFlowId, () => publishingRouter.Publish(Queues.Payments, paymentsEvent, paymentsFlowId, Queues.APIGateway));
            payments = paymentsResponse.Payments;

            var parallelTasks = new List<Task>();
            parallelTasks.Add(Task.Run(async () =>
            {
                var loansFlowId = flowId + "_l";
                var paymentsIds = payments.Select(p => p.Id).ToArray();
                var loansEvent = new GetLoansByPaymentsEvent { PaymentsIds = paymentsIds };
                var loansResponse = await eventsAwaiter.AwaitResponse<SelectedLoansEvent>(loansFlowId, () => publishingRouter.Publish(Queues.Loans, loansEvent, loansFlowId, Queues.APIGateway));
                loans = loansResponse.Loans;
            }));
            parallelTasks.Add(Task.Run(async () =>
            {
                var balancesFlowId = flowId + "_b";
                var accountsIds = payments.Select(p => p.AccountId).ToArray();
                var balancesEvent = new GetBalancesEvent { Ids = accountsIds };
                var balancesRequest = await eventsAwaiter.AwaitResponse<SelectedBalancesEvent>(balancesFlowId, () => publishingRouter.Publish(Queues.Accounts, balancesEvent, balancesFlowId, Queues.APIGateway));
                balances = balancesRequest.Balances;
            }));

            await Task.WhenAll(parallelTasks);

            return new BatchData
            {
                Balances = balances.Select(b => mapper.Map<BalanceDTO>(b)).ToArray(),
                Loans = loans.Select(l => mapper.Map<LoanDTO>(l)).ToArray(),
                Payments = payments.Select(p => mapper.Map<PaymentDTO>(p)).ToArray()
            };
        }

        [HttpPost]
        public Task Process(BatchProcess data)
        {
            var flowId = HttpContext.Items["flowId"].ToString();
            var messages = data.Messages.Select(m => mapper.Map<UserMessage>(m)).ToArray();
            if (messages.Length > 0)
            {
                var messagesEvent = new BatchAddMessagesEvent { Messages = messages };
                publishingRouter.Publish(Queues.Users, messagesEvent, flowId);
            }

            if (data.Transfers.Length > 0)
            {
                var transfersEvent = new BatchTransferEvent { Transfers = data.Transfers };
                publishingRouter.Publish(Queues.Accounts, transfersEvent, flowId);

                var paymentsIds = data.Transfers.Select(t => t.PaymentId);
                var repayTimestampEvent = new UpdateRepayTimestampEvent { Ids = paymentsIds.ToArray(), Timestamp = DateTime.UtcNow };
                publishingRouter.Publish(Queues.Payments, repayTimestampEvent, flowId);
            }

            var instalments = data.RepaidInstalmentsIds;
            if (instalments.Length > 0)
            {
                var transfersEvent = new BatchRepayInstalmentsEvent { Ids = instalments };
                publishingRouter.Publish(Queues.Loans, transfersEvent, flowId);
            }

            return Task.CompletedTask;
        }
    }
}
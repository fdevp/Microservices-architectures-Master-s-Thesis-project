using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using LoansMicroservice;
using Microsoft.Extensions.Logging;
using PaymentsMicroservice.Repository;
using TransactionsMicroservice;
using static LoansMicroservice.Loans;
using static TransactionsMicroservice.Transactions;

namespace PaymentsMicroservice
{
    public class PaymentsService : Payments.PaymentsBase
    {
        private readonly ILogger<PaymentsService> logger;
        private readonly Mapper mapper;
        private readonly TransactionsClient transactionsClient;
        private readonly LoansClient loansClient;
        private readonly PaymentsRepository paymentsRepository;

        public PaymentsService(PaymentsRepository paymentsRepository, ILogger<PaymentsService> logger, Mapper mapper, TransactionsClient transactionsClient, LoansClient loansClient)
        {
            this.paymentsRepository = paymentsRepository;
            this.logger = logger;
            this.mapper = mapper;
            this.transactionsClient = transactionsClient;
            this.loansClient = loansClient;
        }

        public override Task<GetPaymentsResult> Get(GetPaymentsRequest request, ServerCallContext context)
        {
            var payments = request.Ids.Select(id => paymentsRepository.Get(id))
                .Where(payment => payment != null)
                .Select(p => mapper.Map<Payment>(p))
                .ToArray();

            return Task.FromResult(new GetPaymentsResult { Payments = { payments } });
        }

        public override async Task<GetPaymentsWithLoansResult> GetWithLoans(GetPaymentsWithLoansRequest request, ServerCallContext context)
        {
            var payments = paymentsRepository.Get(request.Mod)
                .Where(payment => payment != null)
                .Select(p => mapper.Map<Payment>(p))
                .ToArray();

            var paymentsIds = payments.Select(p => p.Id);
            var loansRequest = new GetPaymentsLoansRequest { FlowId = request.FlowId, PaymentsIds = { paymentsIds } };
            var loansResult = await loansClient.GetPaymentsLoansAsync(loansRequest);

            return new GetPaymentsWithLoansResult { Payments = { payments }, Loans = { loansResult.Loans } };
        }

        public override Task<CreatePaymentResult> Create(CreatePaymentRequest request, ServerCallContext context)
        {
            var payment = paymentsRepository.Create(request.Amount, request.StartTimestamp, request.Interval, request.AccountId, request.Recipient);
            return Task.FromResult(new CreatePaymentResult { Payment = mapper.Map<Payment>(payment) });
        }

        public override Task<Empty> Cancel(CancelPaymentsRequest request, ServerCallContext context)
        {
            foreach (var id in request.Ids)
                paymentsRepository.Cancel(id);
            return Task.FromResult(new Empty());
        }

        public override async Task<GetTransactionsResult> GetTransactions(GetTransactionsRequest request, ServerCallContext context)
        {
            var transactionsRequest = new FilterTransactionsRequest { FlowId = request.FlowId, Payments = { request.Ids } };
            var transactionsResponse = await transactionsClient.FilterAsync(transactionsRequest);
            return new GetTransactionsResult { Transactions = { transactionsResponse.Transactions } };
        }

        public override Task<Empty> Setup(SetupRequest request, Grpc.Core.ServerCallContext context)
        {
            var payments = request.Payments.Select(p => mapper.Map<Repository.Payment>(p));
            paymentsRepository.Setup(payments);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> TearDown(Empty request, Grpc.Core.ServerCallContext context)
        {
            paymentsRepository.TearDown();
            return Task.FromResult(new Empty());
        }
    }
}

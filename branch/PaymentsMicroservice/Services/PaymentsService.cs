using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using LoansMicroservice;
using Microsoft.Extensions.Logging;
using PaymentsMicroservice.Repository;
using SharedClasses;
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

        public override Task<GetPaymentsWithLoansResult> GetByAccounts(GetPaymentsRequest request, ServerCallContext context)
        {
            var payments = paymentsRepository.GetByAccounts(request.Ids);
            return WithLoans(payments, context.RequestHeaders.SelectCustom());
        }

        public override Task<GetPaymentsWithLoansResult> GetPart(GetPartRequest request, ServerCallContext context)
        {
            var payments = paymentsRepository.Get(request.Part, request.TotalParts, request.Timestamp.ToDateTime());
            return WithLoans(payments, context.RequestHeaders.SelectCustom());
        }

        public override Task<Empty> UpdateLatestProcessingTimestamp(UpdateLatestProcessingTimestampRequest request, ServerCallContext context)
        {
            paymentsRepository.UpdateProcessingTimestamp(request.Ids, request.LatestProcessingTimestamp.ToDateTime());
            return Task.FromResult(new Empty());
        }

        public override Task<CreatePaymentResult> Create(CreatePaymentRequest request, ServerCallContext context)
        {
            var payment = paymentsRepository.Create(request.Amount, request.StartTimestamp.ToDateTime(), request.Interval.ToTimeSpan(), request.AccountId, request.Recipient);
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
            var ids = request.Ids.Any() ? request.Ids.ToArray() : paymentsRepository.GetIds();
            var transactionsRequest = new FilterTransactionsRequest { Payments = { ids }, TimestampFrom = request.TimestampFrom, TimestampTo = request.TimestampTo };
            var transactionsResponse = await transactionsClient.FilterAsync(transactionsRequest, context.RequestHeaders.SelectCustom());
            return new GetTransactionsResult { Transactions = { transactionsResponse.Transactions } };
        }

        public override Task<Empty> Setup(SetupRequest request, Grpc.Core.ServerCallContext context)
        {
            var payments = request.Payments.Select(p => mapper.Map<Repository.Payment>(p));
            paymentsRepository.Setup(payments);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> SetupAppend(SetupRequest request, Grpc.Core.ServerCallContext context)
        {
            var payments = request.Payments.Select(p => mapper.Map<Repository.Payment>(p));
            paymentsRepository.SetupAppend(payments);
            return Task.FromResult(new Empty());
        }

        private async Task<GetPaymentsWithLoansResult> WithLoans(Repository.Payment[] payments, Metadata headers)
        {
            var mapped = payments.Where(payment => payment != null)
                            .Select(p => mapper.Map<Payment>(p))
                            .ToArray();

            var paymentsIds = payments.Select(p => p.Id);
            var loansRequest = new GetLoansRequest { Ids = { paymentsIds } };
            var loansResult = await loansClient.GetByPaymentsAsync(loansRequest, headers);

            return new GetPaymentsWithLoansResult { Payments = { mapped }, Loans = { loansResult.Loans } };
        }
    }
}

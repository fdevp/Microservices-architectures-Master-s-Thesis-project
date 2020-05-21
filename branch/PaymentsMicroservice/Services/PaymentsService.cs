using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using PaymentsMicroservice.Repository;
using TransactionsMicroservice;
using static TransactionsMicroservice.Transactions;

namespace PaymentsMicroservice
{
    public class PaymentsService : Payments.PaymentsBase
    {
        private readonly ILogger<PaymentsService> _logger;
        private readonly Mapper mapper;
        private readonly TransactionsClient transactionsClient;
        private readonly PaymentsRepository repository = new PaymentsRepository();

        public PaymentsService(ILogger<PaymentsService> logger, Mapper mapper, TransactionsClient transactionsClient)
        {
            _logger = logger;
            this.mapper = mapper;
            this.transactionsClient = transactionsClient;
        }

        public override Task<GetPaymentsResult> Get(GetPaymentsRequest request, ServerCallContext context)
        {
            var payments = request.Ids.Select(id => repository.Get(id))
                .Where(payment => payment != null)
                .Select(p => mapper.Map<Payment>(p))
                .ToArray();

            return Task.FromResult(new GetPaymentsResult { Payments = { payments } });
        }

        public override Task<CreatePaymentResult> Create(CreatePaymentRequest request, ServerCallContext context)
        {
            var payment = repository.Create(request.Amount, request.StartTimestamp, request.Interval, request.AccountId, request.Recipient);
            return Task.FromResult(new CreatePaymentResult { Payment = mapper.Map<Payment>(payment) });
        }

        public override Task<Empty> Cancel(CancelPaymentRequest request, ServerCallContext context)
        {
            repository.Cancel(request.Id);
            return Task.FromResult(new Empty());
        }

        public override async Task<GetTransactionsResult> GetTransactions(GetTransactionsRequest request, ServerCallContext context)
        {
            var transactionsRequest = new FilterTransactionsRequest { Payments = { request.Ids } };
            var transactionsResponse = await transactionsClient.FilterAsync(transactionsRequest);
            return new GetTransactionsResult { Transactions = { transactionsResponse.Transactions } };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using PaymentsMicroservice.Repository;

namespace PaymentsMicroservice
{
    public class PaymentsService : Payments.PaymentsBase
    {
        private readonly ILogger<PaymentsService> _logger;
        private readonly Mapper mapper;
        private readonly PaymentsRepository repository = new PaymentsRepository();

        public PaymentsService(ILogger<PaymentsService> logger, Mapper mapper)
        {
            _logger = logger;
            this.mapper = mapper;
        }

        public override async Task<GetPaymentsResult> Get(GetPaymentsRequest request, ServerCallContext context)
        {
            var payments = request.Ids.Select(id => repository.Get(id))
                .Where(payment => payment != null)
                .Select(p => mapper.Map<Payment>(p))
                .ToArray();

            return new GetPaymentsResult { Payments = { payments } };
        }

        public override async Task<GetTransactionsResult> GetTransactions(GetTransactionsRequest request, ServerCallContext context)
        {

        }

        public override async Task<CreatePaymentResult> Create(CreatePaymentRequest request, ServerCallContext context)
        {
            var payment = repository.Create(request.Amount, request.StartTimestamp, request.Interval, request.AccountId, request.Recipient);
            return new CreatePaymentResult { Payment = mapper.Map<Payment>(payment) };
        }

        public override async Task<Empty> Cancel(CancelPaymentRequest request, ServerCallContext context)
        {
            repository.Cancel(request.Id);
            return new Empty();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using LoansMicroservice.Repository;
using Microsoft.Extensions.Logging;
using PaymentsMicroservice;
using static PaymentsMicroservice.Payments;

namespace LoansMicroservice
{
    public class LoansService : Loans.LoansBase
    {
        private readonly ILogger<LoansService> logger;
        private readonly Mapper mapper;
        private readonly PaymentsClient paymentsClient;
        private LoansRepository repository = new LoansRepository();

        public LoansService(ILogger<LoansService> logger, Mapper mapper, PaymentsClient paymentsClient)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.paymentsClient = paymentsClient;
        }

        public override Task<GetLoansResponse> Get(GetLoansRequest request, ServerCallContext context)
        {
            var loans = request.Ids.Select(id => repository.Get(id))
                .Where(loan => loan != null)
                .Select(loan => mapper.Map<Loan>(loan));
            return Task.FromResult(new GetLoansResponse { Loans = { loans } });
        }

        public override async Task<Empty> BatchRepayInstalments(BatchRepayInstalmentsRequest request, ServerCallContext context)
        {
            var paymentsToFinish = new List<string>();
            foreach (var id in request.Ids)
            {
                var totalAmountPaid = repository.RepayInstalment(id);
                if (totalAmountPaid)
                    paymentsToFinish.Add(repository.Get(id).PaymentId);
            }

            var cancelPaymentsRequest = new CancelPaymentsRequest
            {
                FlowId = request.FlowId,
                Ids = { paymentsToFinish }
            };
            await paymentsClient.CancelAsync(cancelPaymentsRequest);
            return new Empty();
        }

        public override Task<Empty> Setup(SetupRequest request, ServerCallContext context)
        {
            var loans = request.Loans.Select(l => mapper.Map<Repository.Loan>(l));
            repository.Setup(loans);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> TearDown(Empty request, ServerCallContext context)
        {
            repository.TearDown();
            return Task.FromResult(new Empty());
        }
    }
}

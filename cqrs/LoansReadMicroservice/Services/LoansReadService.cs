using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using LoansReadMicroservice.Repository;
using Microsoft.Extensions.Logging;
using static PaymentsReadMicroservice.PaymentsRead;

namespace LoansReadMicroservice
{
    public class LoansReadService : LoansRead.LoansReadBase
    {
        private readonly ILogger<LoansReadService> logger;
        private readonly Mapper mapper;
        private readonly PaymentsReadClient paymentsClient;
        private LoansRepository loansRepository;

        public LoansReadService(LoansRepository loansRepository, ILogger<LoansReadService> logger, Mapper mapper, PaymentsReadClient paymentsClient)
        {
            this.loansRepository = loansRepository;
            this.logger = logger;
            this.mapper = mapper;
            this.paymentsClient = paymentsClient;
        }

        public override Task<GetLoansResponse> Get(GetLoansRequest request, ServerCallContext context)
        {
            var loans = request.Ids.Select(id => loansRepository.Get(id))
                .Where(loan => loan != null)
                .Select(loan => mapper.Map<Loan>(loan));
            return Task.FromResult(new GetLoansResponse { Loans = { loans } });
        }

        public override Task<GetLoansResponse> GetLoansByPayments(GetLoansByPaymentsRequest request, ServerCallContext context)
        {
            var loans = loansRepository.GetByPayment(request.PaymentsIds)
                .Select(loan => mapper.Map<Loan>(loan));
            return Task.FromResult(new GetLoansResponse { Loans = { loans } });
        }
    }
}

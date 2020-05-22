using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using LoansMicroservice.Repository;
using Microsoft.Extensions.Logging;

namespace LoansMicroservice
{
    public class LoansService : Loans.LoansBase
    {
        private readonly ILogger<LoansService> logger;
        private readonly Mapper mapper;
        private LoansRepository loansRepository = new LoansRepository();

        public LoansService(ILogger<LoansService> logger, Mapper mapper)
        {
            this.logger = logger;
            this.mapper = mapper;
        }

        public override Task<Empty> Setup(SetupRequest request, ServerCallContext context)
        {
            var loans = request.Loans.Select(l => mapper.Map<Repository.Loan>(l));
            loansRepository.Setup(loans);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> TearDown(Empty request, ServerCallContext context)
        {
            loansRepository.TearDown();
            return Task.FromResult(new Empty());
        }
    }
}

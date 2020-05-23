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
        private LoansRepository repository = new LoansRepository();

        public LoansService(ILogger<LoansService> logger, Mapper mapper)
        {
            this.logger = logger;
            this.mapper = mapper;
        }

        public override Task<GetLoansResponse> Get(GetLoansRequest request, ServerCallContext context)
        {
            var loans = request.Ids.Select(id => repository.Get(id))
                .Where(loan => loan != null)
                .Select(loan => mapper.Map<Loan>(loan));
            return Task.FromResult(new GetLoansResponse { Loans = { loans } });
        }

        // public override RepayInstalment(RepayInstalmentRequest request, ServerCallContext context)
        // {
        //     if (repository.Get(request.Id) == null)
        //         throw new ArgumentException("Loan not found.");
        //     var amount = repository.InstalmentAmount(request.Id);

        // }

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

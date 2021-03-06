using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using LoansMicroservice.Repository;
using Microsoft.Extensions.Logging;
using PaymentsMicroservice;
using SharedClasses;
using TransactionsMicroservice;
using static PaymentsMicroservice.Payments;
using static TransactionsMicroservice.Transactions;

namespace LoansMicroservice
{
    public class LoansService : Loans.LoansBase
    {
        private readonly ILogger<LoansService> logger;
        private readonly Mapper mapper;
        private readonly PaymentsClient paymentsClient;
        private readonly TransactionsClient transactionsClient;
        private LoansRepository loansRepository;

        public LoansService(LoansRepository loansRepository, ILogger<LoansService> logger, Mapper mapper, PaymentsClient paymentsClient, TransactionsClient transactionsClient)
        {
            this.loansRepository = loansRepository;
            this.logger = logger;
            this.mapper = mapper;
            this.paymentsClient = paymentsClient;
            this.transactionsClient = transactionsClient;
        }

        public override Task<GetLoansResponse> Get(GetLoansRequest request, ServerCallContext context)
        {
            var loans = request.Ids.Select(id => loansRepository.Get(id))
                .Where(loan => loan != null)
                .Select(loan => mapper.Map<Loan>(loan));
            return Task.FromResult(new GetLoansResponse { Loans = { loans } });
        }

        public override Task<GetLoansResponse> GetByPayments(GetLoansRequest request, ServerCallContext context)
        {
            var loans = loansRepository.GetByPayments(request.Ids)
                .Select(loan => mapper.Map<Loan>(loan));
            return Task.FromResult(new GetLoansResponse { Loans = { loans } });
        }

        public override Task<GetLoansResponse> GetByAccounts(GetLoansRequest request, ServerCallContext context)
        {
            var loans = loansRepository.GetByAccounts(request.Ids)
                .Select(loan => mapper.Map<Loan>(loan));
            return Task.FromResult(new GetLoansResponse { Loans = { loans } });
        }

        public async override Task<GetTransactionsResult> GetTransactions(GetTransactionsRequest request, ServerCallContext context)
        {
            var ids = request.Ids.Any() ? request.Ids.ToArray() : loansRepository.GetPaymentsIds();
            var transactionsRequest = new FilterTransactionsRequest { Payments = { ids }, TimestampFrom = request.TimestampFrom, TimestampTo = request.TimestampTo };
            var transactionsResponse = await transactionsClient.FilterAsync(transactionsRequest, context.RequestHeaders.SelectCustom());
            return new GetTransactionsResult { Transactions = { transactionsResponse.Transactions } };
        }

        public override async Task<Empty> BatchRepayInstalments(BatchRepayInstalmentsRequest request, ServerCallContext context)
        {
            var paymentsToFinish = RepayInstalments(request);
            if (paymentsToFinish.Any())
            {
                var cancelPaymentsRequest = new CancelPaymentsRequest { Ids = { paymentsToFinish } };
                await paymentsClient.CancelAsync(cancelPaymentsRequest, context.RequestHeaders.SelectCustom());
            }
            return new Empty();
        }

        public override Task<Empty> Setup(SetupRequest request, ServerCallContext context)
        {
            var loans = request.Loans.Select(l => mapper.Map<Repository.Loan>(l));
            loansRepository.Setup(loans);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> SetupAppend(SetupRequest request, ServerCallContext context)
        {
            var loans = request.Loans.Select(l => mapper.Map<Repository.Loan>(l));
            loansRepository.SetupAppend(loans);
            return Task.FromResult(new Empty());
        }

        private IEnumerable<string> RepayInstalments(BatchRepayInstalmentsRequest request)
        {
            foreach (var id in request.Ids)
            {
                var totalAmountPaid = loansRepository.RepayInstalment(id);
                if (totalAmountPaid)
                    yield return loansRepository.Get(id).PaymentId;
            }
        }
    }
}

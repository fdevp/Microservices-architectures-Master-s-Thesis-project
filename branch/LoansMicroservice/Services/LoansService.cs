using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using LoansMicroservice.Repository;
using Microsoft.Extensions.Logging;
using PaymentsMicroservice;
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

        public override Task<GetLoansResponse> GetLoansByPayments(GetLoansByPaymentsRequest request, ServerCallContext context)
        {
            var loans = loansRepository.GetByPayment(request.PaymentsIds)
                .Select(loan => mapper.Map<Loan>(loan));
            return Task.FromResult(new GetLoansResponse { Loans = { loans } });
        }

        public async override Task<GetTransactionsResult> GetTransactions(GetTransactionsRequest request, ServerCallContext context)
        {
            var transactionsRequest = new FilterTransactionsRequest { FlowId = request.FlowId, Payments = { request.Ids }, TimestampFrom = request.TimestampFrom, TimestampTo = request.TimestampTo };
            var transactionsResponse = await transactionsClient.FilterAsync(transactionsRequest);
            return new GetTransactionsResult { Transactions = { transactionsResponse.Transactions } };
        }

        public override async Task<Empty> BatchRepayInstalments(BatchRepayInstalmentsRequest request, ServerCallContext context)
        {
            var paymentsToFinish = new List<string>();
            foreach (var id in request.Ids)
            {
                var totalAmountPaid = loansRepository.RepayInstalment(id);
                if (totalAmountPaid)
                    paymentsToFinish.Add(loansRepository.Get(id).PaymentId);
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
            loansRepository.Setup(loans);
            return Task.FromResult(new Empty());
        }
    }
}

using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Core;
using LoansReadMicroservice;
using Microsoft.Extensions.Logging;
using PaymentsReadMicroservice.Repository;
using TransactionsReadMicroservice;
using static LoansReadMicroservice.LoansRead;
using static TransactionsReadMicroservice.TransactionsRead;

namespace PaymentsReadMicroservice
{
    public class PaymentsReadService : PaymentsRead.PaymentsReadBase
    {
        private readonly ILogger<PaymentsReadService> logger;
        private readonly Mapper mapper;
        private readonly TransactionsReadClient transactionsClient;
        private readonly LoansReadClient loansClient;
        private readonly PaymentsRepository paymentsRepository;

        public PaymentsReadService(PaymentsRepository paymentsRepository, ILogger<PaymentsReadService> logger, Mapper mapper, TransactionsReadClient transactionsClient, LoansReadClient loansClient)
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
            var payments = request.AccountIds.Any() ? paymentsRepository.GetByAccounts(request.AccountIds) : paymentsRepository.Get(request.Part, request.TotalParts);
            var mapped = payments.Where(payment => payment != null)
                .Select(p => mapper.Map<Payment>(p))
                .ToArray();

            var paymentsIds = payments.Select(p => p.Id);
            var loansRequest = new GetPaymentsLoansRequest { FlowId = request.FlowId, PaymentsIds = { paymentsIds } };
            var loansResult = await loansClient.GetPaymentsLoansAsync(loansRequest);

            return new GetPaymentsWithLoansResult { Payments = { mapped }, Loans = { loansResult.Loans } };
        }

        public override async Task<GetTransactionsResult> GetTransactions(GetTransactionsRequest request, ServerCallContext context)
        {
            var transactionsRequest = new FilterTransactionsRequest { FlowId = request.FlowId, Payments = { request.Ids } };
            var transactionsResponse = await transactionsClient.FilterAsync(transactionsRequest);
            return new GetTransactionsResult { Transactions = { transactionsResponse.Transactions } };
        }
    }
}

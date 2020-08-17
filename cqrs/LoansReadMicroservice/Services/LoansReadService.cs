using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using LoansReadMicroservice.Repository;
using Microsoft.Extensions.Logging;
using SharedClasses;
using TransactionsReadMicroservice;
using static PaymentsReadMicroservice.PaymentsRead;
using static TransactionsReadMicroservice.TransactionsRead;

namespace LoansReadMicroservice
{
    public class LoansReadService : LoansRead.LoansReadBase
    {
        private readonly ILogger<LoansReadService> logger;
        private readonly Mapper mapper;
        private readonly PaymentsReadClient paymentsClient;
        private readonly TransactionsReadClient transactionsReadClient;
        private LoansRepository loansRepository;

        public LoansReadService(LoansRepository loansRepository, ILogger<LoansReadService> logger, Mapper mapper, PaymentsReadClient paymentsClient, TransactionsReadClient transactionsReadClient)
        {
            this.loansRepository = loansRepository;
            this.logger = logger;
            this.mapper = mapper;
            this.paymentsClient = paymentsClient;
            this.transactionsReadClient = transactionsReadClient;
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

        public override async Task<AggregateOverallResponse> AggregateOverall(AggregateOverallRequest request, ServerCallContext context)
        {
            var paymentsIds = loansRepository.GetPaymentsIds();
            var transactionsResponse = await transactionsReadClient.FilterAsync(new FilterTransactionsRequest { Payments = { paymentsIds }, TimestampTo = request.TimestampTo, TimestampFrom = request.TimestampFrom }, context.RequestHeaders.SelectCustom());
            var aggregations = Aggregations.CreateOverallCsvReport(new OverallReportData { Aggregations = request.Aggregations.ToArray(), Granularity = request.Granularity, Transactions = transactionsResponse.Transactions.ToArray() });
            return new AggregateOverallResponse { Portions = { aggregations } };
        }

        public override async Task<AggregateUserActivityResponse> AggregateUserActivity(AggregateUserActivityRequest request, ServerCallContext context)
        {
            var loans = loansRepository.GetByAccounts(request.AccountsIds);
            var paymentsIds = loans.Select(l => l.PaymentId).ToArray();
            var transactionsResponse = await transactionsReadClient.FilterAsync(new FilterTransactionsRequest { Payments = { paymentsIds }, TimestampFrom = request.TimestampFrom, TimestampTo = request.TimestampTo }, context.RequestHeaders.SelectCustom());
            var transactions = transactionsResponse.Transactions.ToArray();
            var aggregated = loans.SelectMany(l => AggregateUserTransactions(l, transactions.Where(t => t.PaymentId == l.PaymentId).ToArray(), request.Granularity));
            return new AggregateUserActivityResponse { Portions = { aggregated } };
        }

        private IEnumerable<UserReportPortion> AggregateUserTransactions(Repository.Loan loan, Transaction[] transactions, Granularity granularity)
        {
            var withTimestamps = transactions.Select(t => new TransactionWithTimestamp { Timestamp = t.Timestamp.ToDateTime(), Transaction = t });
            var portions = Aggregations.GroupByPeriods(granularity, withTimestamps);
            var ordered = portions.OrderBy(p => p.Key);
            foreach (var portion in ordered)
            {
                var debits = portion.Where(p => p.Transaction.PaymentId == loan.PaymentId).Sum(p => (float?)p.Transaction.Amount) ?? 0;
                yield return new UserReportPortion { Period = portion.Key, Debits = debits, Element = loan.Id };
            }
        }
    }
}

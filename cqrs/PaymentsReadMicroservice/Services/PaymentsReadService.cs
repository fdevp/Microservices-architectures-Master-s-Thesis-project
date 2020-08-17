using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using LoansReadMicroservice;
using Microsoft.Extensions.Logging;
using PaymentsReadMicroservice.Repository;
using SharedClasses;
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

        public override Task<GetPaymentsResult> GetPart(GetPartRequest request, ServerCallContext context)
        {
            var payments = paymentsRepository.Get(request.Part, request.TotalParts);
            var mapped = payments.Where(payment => payment != null)
                .Select(p => mapper.Map<Payment>(p))
                .ToArray();
            return Task.FromResult(new GetPaymentsResult { Payments = { mapped } });
        }

        public override Task<GetPaymentsResult> GetByAccounts(GetPaymentsRequest request, ServerCallContext context)
        {
            var payments = paymentsRepository.GetByAccounts(request.Ids)
                .Select(p => mapper.Map<Payment>(p))
                .ToArray();
            return Task.FromResult(new GetPaymentsResult { Payments = { payments } });
        }

        public override async Task<AggregateOverallResponse> AggregateOverall(AggregateOverallRequest request, ServerCallContext context)
        {
            var paymentsIds = paymentsRepository.GetIds();
            var transactionsResponse = await transactionsClient.FilterAsync(new FilterTransactionsRequest { Payments = { paymentsIds }, TimestampTo = request.TimestampTo, TimestampFrom = request.TimestampFrom }, context.RequestHeaders.SelectCustom());
            var aggregations = Aggregations.CreateOverallCsvReport(new OverallReportData { Aggregations = request.Aggregations.ToArray(), Granularity = request.Granularity, Transactions = transactionsResponse.Transactions.ToArray() });
            return new AggregateOverallResponse { Portions = { aggregations } };
        }

        public override async Task<AggregateUserActivityResponse> AggregateUserActivity(AggregateUserActivityRequest request, ServerCallContext context)
        {
            var payments = paymentsRepository.GetByAccounts(request.AccountsIds);
            var paymentsIds = payments.Select(p => p.Id).ToArray();
            var transactionsResponse = await transactionsClient.FilterAsync(new FilterTransactionsRequest { Payments = { paymentsIds }, TimestampFrom = request.TimestampFrom, TimestampTo = request.TimestampTo }, context.RequestHeaders.SelectCustom());
            var transactions = transactionsResponse.Transactions.ToArray();
            var aggregated = payments.SelectMany(p => AggregateUserTransactions(p, transactions.Where(t => t.PaymentId == p.Id).ToArray(), request.Granularity));
            return new AggregateUserActivityResponse { Portions = { aggregated } };
        }

        private IEnumerable<UserReportPortion> AggregateUserTransactions(Repository.Payment payment, Transaction[] transactions, Granularity granularity)
        {
            var withTimestamps = transactions.Select(t => new TransactionWithTimestamp { Timestamp = t.Timestamp.ToDateTime(), Transaction = t });
            var portions = Aggregations.GroupByPeriods(granularity, withTimestamps);
            var ordered = portions.OrderBy(p => p.Key);
            foreach (var portion in ordered)
            {
                var debits = portion.Where(p => p.Transaction.PaymentId == payment.Id).Sum(p => (float?)p.Transaction.Amount) ?? 0;
                yield return new UserReportPortion { Period = portion.Key, Debits = debits, Element = payment.Id };
            }
        }
    }
}
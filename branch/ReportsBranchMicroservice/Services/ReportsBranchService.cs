using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using SharedClasses;

namespace ReportsBranchMicroservice
{
    public class ReportsBranchService : ReportsBranch.ReportsBranchBase
    {
        private readonly ILogger<ReportsBranchService> logger;
        private readonly DataFetcher dataFetcher;

        public ReportsBranchService(ILogger<ReportsBranchService> logger, DataFetcher dataFetcher)
        {
            this.logger = logger;
            this.dataFetcher = dataFetcher;
        }

        public override async Task<AggregateOverallResponse> AggregateOverall(AggregateOverallRequest request, ServerCallContext context)
        {
            var headers = context.RequestHeaders.SelectCustom();
            Transaction[] transactions;

            switch (request.Subject)
            {
                case ReportSubject.Cards:
                    transactions = await dataFetcher.GetCardsTransactions(headers, request.TimestampFrom, request.TimestampTo);
                    break;
                case ReportSubject.Loans:
                    transactions = await dataFetcher.GetLoansTransactions(headers, request.TimestampFrom, request.TimestampTo);
                    break;
                case ReportSubject.Payments:
                    transactions = await dataFetcher.GetPaymentsTransactions(headers, request.TimestampFrom, request.TimestampTo);
                    break;
                case ReportSubject.Transactions:
                    transactions = await dataFetcher.GetTransactions(headers, request.TimestampFrom, request.TimestampTo);
                    break;
                default:
                    throw new InvalidOperationException("Unknown subject of report.");
            }

            var data = new OverallReportData
            {
                From = request.TimestampFrom.ToDateTime(),
                To = request.TimestampTo.ToDateTime(),
                Granularity = request.Granularity,
                Subject = request.Subject,
                Aggregations = request.Aggregations.ToArray(),
                Transactions = transactions
            };

            var report = ReportGenerator.AggregateOverall(data);
            return new AggregateOverallResponse { Report = report };
        }

        public override async Task<AggregateUserActivityResponse> AggregateUserActivity(AggregateUserActivityRequest request, ServerCallContext context)
        {
            var headers = context.RequestHeaders.SelectCustom();
            var accounts = await dataFetcher.GetAccounts(headers, request.UserId);
            var accountsIds = accounts.Select(a => a.Id).ToArray();
            var data = new UserActivityRaportData
            {
                From = request.TimestampFrom.ToDateTime(),
                To = request.TimestampTo.ToDateTime(),
                Granularity = request.Granularity,
                Accounts = accounts,
                UserId = request.UserId
            };

            var parallelTasks = new List<Task>();
            parallelTasks.Add(Task.Run(async () => data.Transactions = await dataFetcher.GetAccountsTransactions(headers, accountsIds, request.TimestampFrom, request.TimestampTo)));
            parallelTasks.Add(Task.Run(async () =>
            {
                var paymentsAndLoans = await dataFetcher.GetPaymentsWithLoans(headers, accountsIds);
                data.Payments = paymentsAndLoans.Payments;
                data.Loans = paymentsAndLoans.Loans;
            }));
            parallelTasks.Add(Task.Run(async () => data.Cards = await dataFetcher.GetCards(headers, accountsIds)));
            await Task.WhenAll(parallelTasks);

            var report = ReportGenerator.AggregateUserActivity(data);
            return new AggregateUserActivityResponse { Report = report };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;

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

        public override async Task<GenerateReportResponse> GenerateOverallReport(GenerateOverallReportRequest request, ServerCallContext context)
        {
            Transaction[] transactions;

            switch (request.Subject)
            {
                case ReportSubject.Cards:
                    transactions = await dataFetcher.GetCardsTransactions(request.FlowId, request.TimestampFrom, request.TimestampTo);
                    break;
                case ReportSubject.Loans:
                    transactions = await dataFetcher.GetLoansTransactions(request.FlowId, request.TimestampFrom, request.TimestampTo);
                    break;
                case ReportSubject.Payments:
                    transactions = await dataFetcher.GetPaymentsTransactions(request.FlowId, request.TimestampFrom, request.TimestampTo);
                    break;
                case ReportSubject.Transactions:
                    transactions = await dataFetcher.GetTransactions(request.FlowId, request.TimestampFrom, request.TimestampTo);
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
            var csv = ReportGenerator.CreateOverallCsvReport(data);
            return new GenerateReportResponse { FlowId = request.FlowId, Report = csv };
        }

        public override async Task<GenerateReportResponse> GenerateUserActivityReport(GenerateUserActivityReportRequest request, ServerCallContext context)
        {
            var accounts = await dataFetcher.GetAccounts(request.FlowId, request.UserId);
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
            parallelTasks.Add(Task.Run(async () => data.Transactions = await dataFetcher.GetAccountsTransactions(request.FlowId, accountsIds, request.TimestampFrom, request.TimestampTo)));
            parallelTasks.Add(Task.Run(async () =>
            {
                var paymentsAndLoans = await dataFetcher.GetPaymentsWithLoans(request.FlowId, accountsIds);
                data.Payments = paymentsAndLoans.Payments;
                data.Loans = paymentsAndLoans.Loans;
            }));
            parallelTasks.Add(Task.Run(async () => data.Cards = await dataFetcher.GetCards(request.FlowId, accountsIds)));
            await Task.WhenAll(parallelTasks);

            var csv = ReportGenerator.CreateUserActivityCsvReport(data);
            return new GenerateReportResponse { FlowId = request.FlowId, Report = csv };
        }
    }
}

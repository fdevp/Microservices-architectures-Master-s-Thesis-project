using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AccountsMicroservice;
using CardsMicroservice;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using PaymentsMicroservice;
using static AccountsMicroservice.Accounts;
using static CardsMicroservice.Cards;
using static PaymentsMicroservice.Payments;
using static TransactionsMicroservice.Transactions;

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
            string csv = null;
            switch (request.Subject)
            {
                case ReportSubject.Cards:
                    break;
                case ReportSubject.Loans:
                    break;
                case ReportSubject.Payments:
                    break;
                case ReportSubject.Users:
                    break;
                case ReportSubject.Transactions:
                    break;
                default:
                    throw new InvalidOperationException("Unknown subject of report.");
            }
            var report = new Report { Data = ByteString.CopyFromUtf8("asdasd") };
            return new GenerateReportResponse { FlowId = request.FlowId, Report = report };
        }

        public override async Task<GenerateReportResponse> GenerateUserActivityReport(GenerateUserActivityReportRequest request, ServerCallContext context)
        {
            var accounts = await dataFetcher.GetAccounts(request.FlowId, request.UserId);
            var accountsIds = accounts.Select(a => a.Id).ToArray();

            Transaction[] transactions = null;
            PaymentsAndLoans paymentsAndLoans = null;
            Card[] cards = null;

            var parallelTasks = new List<Task>();
            parallelTasks.Add(Task.Run(async () => transactions = await dataFetcher.GetTransactions(request.FlowId, accountsIds)));
            parallelTasks.Add(Task.Run(async () => paymentsAndLoans = await dataFetcher.GetPaymentsWithLoans(request.FlowId, accountsIds)));
            parallelTasks.Add(Task.Run(async () => cards = await dataFetcher.GetCards(request.FlowId, accountsIds)));
            await Task.WhenAll(parallelTasks);

            //generowanie raportu
            var data = new UserActivityRaportData
            {
                From = GetDateTime(request.TimestampFrom),
                To = GetDateTime(request.TimestampTo),
                Granularity = request.Granularity,
                Accounts = accounts,
                Transactions = transactions,
                Cards = cards,
                Loans = paymentsAndLoans.Loans,
                Payments = paymentsAndLoans.Payments
            };

            var csv = ReportGenerator.CreateUserActivityCsvReport(data);
            var report = new Report { Data = ByteString.CopyFromUtf8(csv) };
            return new GenerateReportResponse { FlowId = request.FlowId, Report = report };
        }

        private DateTime? GetDateTime(long ticks) => ticks > 0 ? DateTime.FromBinary(ticks) : null as DateTime?;
    }
}

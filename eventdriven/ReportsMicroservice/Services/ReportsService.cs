using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharedClasses.Events.Reports;
using SharedClasses.Events.Transactions;
using SharedClasses.Messaging;
using SharedClasses.Models;

namespace ReportsMicroservice
{
    public class ReportsService
    {
        private readonly ILogger<ReportsService> logger;
        private readonly PublishingRouter publishingRouter;
        private readonly DataFetcher dataFetcher;

        public ReportsService(ILogger<ReportsService> logger, PublishingRouter publishingRouter, DataFetcher dataFetcher)
        {
            this.logger = logger;
            this.publishingRouter = publishingRouter;
            this.dataFetcher = dataFetcher;
        }

        [EventHandlingMethod(typeof(GenerateOverallReportEvent))]
        public async Task GenerateOverallReport(MessageContext context, GenerateOverallReportEvent inputEvent)
        {
            Transaction[] transactions;
            switch (inputEvent.Subject)
            {
                case ReportSubject.Cards:
                    transactions = await dataFetcher.GetTransactions(context.FlowId + "_c", Queues.Cards, inputEvent.TimestampFrom, inputEvent.TimestampTo);
                    break;
                case ReportSubject.Loans:
                    transactions = await dataFetcher.GetTransactions(context.FlowId + "_l", Queues.Loans, inputEvent.TimestampFrom, inputEvent.TimestampTo);
                    break;
                case ReportSubject.Payments:
                    transactions = await dataFetcher.GetTransactions(context.FlowId + "_p", Queues.Payments, inputEvent.TimestampFrom, inputEvent.TimestampTo);
                    break;
                case ReportSubject.Transactions:
                    transactions = await dataFetcher.GetTransactions(context.FlowId + "_t", new FilterTransactionsEvent { TimestampFrom = inputEvent.TimestampFrom, TimestampTo = inputEvent.TimestampTo });
                    break;
                default:
                    throw new InvalidOperationException("Unknown subject of report.");
            }

            var data = new OverallReportData
            {
                From = inputEvent.TimestampFrom,
                To = inputEvent.TimestampTo,
                Granularity = inputEvent.Granularity,
                Subject = inputEvent.Subject,
                Aggregations = inputEvent.Aggregations,
                Transactions = transactions
            };

            var csv = ReportGenerator.CreateOverallCsvReport(data);
            publishingRouter.Publish(context.ReplyTo, new ReportCreatedEvent { Report = csv }, context.FlowId);


        }

        [EventHandlingMethod(typeof(GenerateUserActivityReportEvent))]
        public async Task GenerateUserActivityReport(MessageContext context, GenerateUserActivityReportEvent inputEvent)
        {
            var accounts = await dataFetcher.GetAccounts(context.FlowId, inputEvent.UserId);
            var accountsIds = accounts.Select(a => a.Id).ToArray();
            var data = new UserActivityRaportData
            {
                From = inputEvent.TimestampFrom,
                To = inputEvent.TimestampTo,
                Granularity = inputEvent.Granularity,
                Accounts = accounts,
                UserId = inputEvent.UserId
            };

            var filters = new FilterTransactionsEvent
            {
                Recipients = accountsIds,
                Senders = accountsIds,
                TimestampFrom = inputEvent.TimestampFrom,
                TimestampTo = inputEvent.TimestampTo
            };

            var parallelTasks = new List<Task>();
            parallelTasks.Add(Task.Run(async () => data.Transactions = await dataFetcher.GetTransactions(context.FlowId + "_t", filters)));
            parallelTasks.Add(Task.Run(async () => data.Payments = await dataFetcher.GetPayments(context.FlowId + "_p", accountsIds)));
            parallelTasks.Add(Task.Run(async () => data.Loans = await dataFetcher.GetLoans(context.FlowId + "_l", accountsIds)));
            parallelTasks.Add(Task.Run(async () => data.Cards = await dataFetcher.GetCards(context.FlowId + "_c", accountsIds)));
            await Task.WhenAll(parallelTasks);

            var csv = ReportGenerator.CreateUserActivityCsvReport(data);
            publishingRouter.Publish(context.ReplyTo, new ReportCreatedEvent { Report = csv }, context.FlowId);
        }
    }
}
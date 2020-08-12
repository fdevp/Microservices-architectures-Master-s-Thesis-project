using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharedClasses.Events;
using SharedClasses.Events.Reports;
using SharedClasses.Events.Transactions;
using SharedClasses.Messaging;
using SharedClasses.Models;
using TransactionsMicroservice.Reports;
using TransactionsMicroservice.Repository;

namespace TransactionsMicroservice
{
    public class TransactionsService
    {
        private readonly ILogger<TransactionsService> logger;
        private readonly PublishingRouter publishingRouter;
        private readonly ReportsDataFetcher reportsDataFetcher;
        private TransactionsRepository transactionsRepository;

        public TransactionsService(TransactionsRepository transactionsRepository, ILogger<TransactionsService> logger, PublishingRouter publishingRouter, ReportsDataFetcher reportsDataFetcher)
        {
            this.transactionsRepository = transactionsRepository;
            this.logger = logger;
            this.publishingRouter = publishingRouter;
            this.reportsDataFetcher = reportsDataFetcher;
        }

        [EventHandlingMethod(typeof(GetTransactionsEvent))]
        public Task Get(MessageContext context, GetTransactionsEvent inputEvent)
        {
            var transactions = inputEvent.Ids.Select(id => transactionsRepository.Get(id))
                .Where(transaction => transaction != null)
                .ToArray();
            publishingRouter.Publish(context.ReplyTo, new SelectedTransactionsEvent { Transactions = transactions }, context.FlowId);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(CreateTransactionEvent))]
        public Task Create(MessageContext context, CreateTransactionEvent inputEvent)
        {
            var transaction = transactionsRepository.Create(inputEvent.Title, inputEvent.Amount, inputEvent.Recipient, inputEvent.Sender, inputEvent.PaymentId, inputEvent.CardId);

            if (context.ReplyTo != null)
                publishingRouter.Publish(context.ReplyTo, new SelectedTransactionsEvent { Transactions = new[] { transaction } }, context.FlowId);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(BatchCreateTransactionEvent))]
        public Task BatchCreate(MessageContext context, BatchCreateTransactionEvent inputEvent)
        {
            var transactions = inputEvent.Requests
                .Select(r => transactionsRepository.Create(r.Title, r.Amount, r.Recipient, r.Sender, r.PaymentId, r.CardId))
                .ToArray();
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(FilterTransactionsEvent))]
        public Task Filter(MessageContext context, FilterTransactionsEvent inputEvent)
        {
            var filters = new Filters
            {
                Cards = inputEvent.Cards?.ToHashSet(),
                Payments = inputEvent.Payments?.ToHashSet(),
                Recipients = inputEvent.Recipients?.ToHashSet(),
                Senders = inputEvent.Senders?.ToHashSet(),
                TimestampFrom = inputEvent.TimestampFrom,
                TimestampTo = inputEvent.TimestampTo,
            };

            var transactions = transactionsRepository.GetMany(filters, inputEvent.Top);
            publishingRouter.Publish(context.ReplyTo, new SelectedTransactionsEvent { Transactions = transactions }, context.FlowId);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(GenerateOverallReportEvent))]
        public async Task GenerateOverallReport(MessageContext context, GenerateOverallReportEvent inputEvent)
        {
            var filters = new Filters { TimestampFrom = inputEvent.TimestampFrom, TimestampTo = inputEvent.TimestampTo };
            var transactions = transactionsRepository.GetMany(inputEvent.Subject, new Filters { TimestampFrom = inputEvent.TimestampFrom, TimestampTo = inputEvent.TimestampTo });
            
            if (inputEvent.Subject == ReportSubject.Loans)
            {
                var accountsIds = transactions.Select(t => t.Sender).ToArray();
                var loans = await reportsDataFetcher.GetLoans(context.FlowId + "_l", accountsIds);
                var loansPayments = loans.Select(l => l.PaymentId).ToHashSet();
                transactions = transactions.Where(t => loansPayments.Contains(t.PaymentId)).ToArray();
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
            publishingRouter.Publish(context.ReplyTo, new ReportCreatedEvent
            {
                Report = csv
            }, context.FlowId);
        }

        [EventHandlingMethod(typeof(GenerateUserActivityReportEvent))]
        public async Task GenerateUserActivityReport(MessageContext context, GenerateUserActivityReportEvent inputEvent)
        {
            var accounts = await reportsDataFetcher.GetAccounts(context.FlowId, inputEvent.UserId);
            var accountsIds = accounts.Select(a => a.Id).ToArray();
            var data = new UserActivityRaportData
            {
                From = inputEvent.TimestampFrom,
                To = inputEvent.TimestampTo,
                Granularity = inputEvent.Granularity,
                Accounts = accounts,
                UserId = inputEvent.UserId
            };

            var filters = new Filters
            {
                Recipients = accountsIds.ToHashSet(),
                Senders = accountsIds.ToHashSet(),
                TimestampFrom = inputEvent.TimestampFrom,
                TimestampTo = inputEvent.TimestampTo
            };

            var parallelTasks = new List<Task>();
            parallelTasks.Add(Task.Run(() => data.Transactions = transactionsRepository.GetMany(filters, null)));
            parallelTasks.Add(Task.Run(async () => data.Payments = await reportsDataFetcher.GetPayments(context.FlowId + "_p", accountsIds)));
            parallelTasks.Add(Task.Run(async () => data.Loans = await reportsDataFetcher.GetLoans(context.FlowId + "_l", accountsIds)));
            parallelTasks.Add(Task.Run(async () => data.Cards = await reportsDataFetcher.GetCards(context.FlowId + "_c", accountsIds)));
            await Task.WhenAll(parallelTasks);

            var csv = ReportGenerator.CreateUserActivityCsvReport(data);
        }

        [EventHandlingMethod(typeof(SetupTransactionsEvent))]
        public Task Setup(MessageContext context, SetupTransactionsEvent inputEvent)
        {
            transactionsRepository.Setup(inputEvent.Transactions);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(SetupAppendTransactionsEvent))]
        public Task SetupAppend(MessageContext context, SetupAppendTransactionsEvent inputEvent)
        {
            transactionsRepository.SetupAppend(inputEvent.Transactions);
            return Task.CompletedTask;
        }
    }
}

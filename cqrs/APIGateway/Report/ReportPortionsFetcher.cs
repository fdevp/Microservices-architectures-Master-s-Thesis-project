using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountsReadMicroservice;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using static AccountsReadMicroservice.AccountsRead;
using static CardsReadMicroservice.CardsRead;
using static LoansReadMicroservice.LoansRead;
using static PaymentsReadMicroservice.PaymentsRead;
using static TransactionsReadMicroservice.TransactionsRead;

namespace APIGateway.Reports
{
    public class ReportDataFetcher
    {
        private readonly AccountsReadClient accountsClient;
        private readonly PaymentsReadClient paymentsClient;
        private readonly TransactionsReadClient transactionsClient;
        private readonly CardsReadClient cardsClient;
        private readonly LoansReadClient loansClient;

        public ReportDataFetcher(AccountsReadClient accountsClient,
         PaymentsReadClient paymentsClient,
         TransactionsReadClient transactionsClient,
         CardsReadClient cardsClient,
         LoansReadClient loansClient)
        {
            this.accountsClient = accountsClient;
            this.paymentsClient = paymentsClient;
            this.transactionsClient = transactionsClient;
            this.cardsClient = cardsClient;
            this.loansClient = loansClient;
        }

        public async Task<OverallReportPortion[]> GetOverallReportPortions(AggregateOverallRequest request, Metadata headers, Models.ReportSubject subject)
        {
            AggregateOverallResponse response;
            switch (subject)
            {
                case Models.ReportSubject.Cards:
                    response = await cardsClient.AggregateOverallAsync(request, headers);
                    break;
                case Models.ReportSubject.Loans:
                    response = await loansClient.AggregateOverallAsync(request, headers);
                    break;
                case Models.ReportSubject.Payments:
                    response = await paymentsClient.AggregateOverallAsync(request, headers);
                    break;
                case Models.ReportSubject.Transactions:
                    response = await transactionsClient.AggregateOverallAsync(request, headers);
                    break;
                default:
                    throw new InvalidOperationException("Unknown subject of report.");
            }

            return response.Portions.ToArray();
        }

        public async Task<ReportPortions> GetUserActivityPortions(Metadata headers, string userId, Timestamp from, Timestamp to, Granularity granularity)
        {
            var portions = new ReportPortions();

            var accountsResponse = await accountsClient.GetUserAccountsAsync(new GetUserAccountsRequest { UserId = userId }, headers);
            var accountsIds = accountsResponse.Accounts.Select(a => a.Id).ToArray();

            var parallelTasks = new List<Task>();
            parallelTasks.Add(Task.Run(async () =>
            {
                var request = new AccountsReadMicroservice.AggregateUserActivityRequest { UserId = userId, TimestampFrom = from, TimestampTo = to, Granularity = granularity };
                var response = await accountsClient.AggregateUserActivityAsync(request, headers);
                portions.Accounts = response.Portions.ToArray();
            }));
            parallelTasks.Add(Task.Run(async () =>
            {
                var request = new PaymentsReadMicroservice.AggregateUserActivityRequest { AccountsIds = { accountsIds }, TimestampFrom = from, TimestampTo = to, Granularity = granularity };
                var response = await paymentsClient.AggregateUserActivityAsync(request, headers);
                portions.Payments = response.Portions.ToArray();
            }));
            parallelTasks.Add(Task.Run(async () =>
            {
                var request = new LoansReadMicroservice.AggregateUserActivityRequest { AccountsIds = { accountsIds }, TimestampFrom = from, TimestampTo = to, Granularity = granularity };
                var response = await loansClient.AggregateUserActivityAsync(request, headers);
                portions.Loans = response.Portions.ToArray();
            }));
            parallelTasks.Add(Task.Run(async () =>
            {
                var request = new CardsReadMicroservice.AggregateUserActivityRequest { AccountsIds = { accountsIds }, TimestampFrom = from, TimestampTo = to, Granularity = granularity };
                var response = await cardsClient.AggregateUserActivityAsync(request, headers);
                portions.Cards = response.Portions.ToArray();
            }));

            await Task.WhenAll(parallelTasks);
            return portions;
        }
    }
}
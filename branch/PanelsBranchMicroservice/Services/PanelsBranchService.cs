using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountsMicroservice;
using CardsMicroservice;
using Google.Protobuf.Collections;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using PaymentsMicroservice;
using TransactionsMicroservice;
using static AccountsMicroservice.Accounts;
using static CardsMicroservice.Cards;
using static LoansMicroservice.Loans;
using static PaymentsMicroservice.Payments;
using static TransactionsMicroservice.Transactions;

namespace PanelsBranchMicroservice
{
    public class PanelsBranchService : PanelsBranch.PanelsBranchBase
    {
        private readonly ILogger<PanelsBranchService> logger;
        private readonly TransactionsClient transactionsClient;
        private readonly PaymentsClient paymentsClient;
        private readonly LoansClient loansClient;
        private readonly AccountsClient accountsClient;
        private readonly CardsClient cardsClient;

        public PanelsBranchService(ILogger<PanelsBranchService> logger,
            TransactionsClient transactionsClient,
            PaymentsClient paymentsClient,
            LoansClient loansClient,
            AccountsClient accountsClient,
            CardsClient cardsClient
        )
        {
            this.logger = logger;
            this.transactionsClient = transactionsClient;
            this.paymentsClient = paymentsClient;
            this.loansClient = loansClient;
            this.accountsClient = accountsClient;
            this.cardsClient = cardsClient;
        }

        public override async Task<GetPanelResponse> Get(GetPanelRequest request, ServerCallContext context)
        {
            RepeatedField<Loan> loans = null;
            RepeatedField<Payment> payments = null;
            RepeatedField<Account> accounts = null;
            RepeatedField<Card> cards = null;
            RepeatedField<Transaction> transactions = null;

            var accountsResponse = await accountsClient.GetUserAccountsAsync(new GetUserAccountsRequest { FlowId = request.FlowId, UserId = request.UserId });
            var accountsIds = accounts.Select(a => a.Id).ToArray();
            accounts = accountsResponse.Accounts;


            var parallelTasks = new List<Task>();
            parallelTasks.Add(new Task(async () =>
            {
                var transactionsResponse = await transactionsClient.FilterAsync(new FilterTransactionsRequest { FlowId = request.FlowId, Senders = { accountsIds } });
                transactions = transactionsResponse.Transactions;
            }));

            parallelTasks.Add(new Task(async () =>
            {
                var paymentsAndLoans = await paymentsClient.GetWithLoansAsync(new GetPaymentsWithLoansRequest { FlowId = request.FlowId, AccountIds = { accountsIds } });
                loans = paymentsAndLoans.Loans;
                payments = paymentsAndLoans.Payments;
            }));

            parallelTasks.Add(new Task(async () =>
            {
                var cardsResponse = await cardsClient.GetByAccountsAsync(new GetCardsByAccountsRequest { FlowId = request.FlowId, AccountIds = { accountsIds } });
                cards = cardsResponse.Cards;
            }));


            await Task.WhenAll(parallelTasks);
            return new GetPanelResponse
            {
                Cards = { cards },
                Payments = { payments },
                Accounts = { accounts },
                Loans = { loans },
                Transactions = { transactions }
            };
        }
    }
}

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
        private const int PanelTransactionsCount = 5;
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
            RepeatedField<Loan> loans = new RepeatedField<Loan>();
            RepeatedField<Payment> payments = new RepeatedField<Payment>();
            RepeatedField<Account> accounts = new RepeatedField<Account>();
            RepeatedField<Card> cards = new RepeatedField<Card>();
            RepeatedField<Transaction> transactions = new RepeatedField<Transaction>();

            var accountsResponse = await accountsClient.GetUserAccountsAsync(new GetUserAccountsRequest { FlowId = request.FlowId, UserId = request.UserId });
            var accountsIds = accountsResponse.Accounts.Select(a => a.Id).ToArray();
            accounts = accountsResponse.Accounts;


            var parallelTasks = new List<Task>();
            parallelTasks.Add(Task.Run(async () =>
            {
                var transactionsResponse = await transactionsClient.FilterAsync(new FilterTransactionsRequest { FlowId = request.FlowId, Senders = { accountsIds }, Top = PanelTransactionsCount });
            }));

            parallelTasks.Add(Task.Run(async () =>
            {
                var paymentsAndLoans = await paymentsClient.GetByAccountAsync(new GetByAccountRequest { FlowId = request.FlowId, AccountIds = { accountsIds } });
                loans = paymentsAndLoans.Loans;
                payments = paymentsAndLoans.Payments;
            }));

            parallelTasks.Add(Task.Run(async () =>
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
                Transactions = { transactions.OrderByDescending(t => t.Timestamp) }
            };
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using AccountsMicroservice;
using CardsMicroservice;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using PaymentsMicroservice;
using static AccountsMicroservice.Accounts;
using static CardsMicroservice.Cards;
using static LoansMicroservice.Loans;
using static PaymentsMicroservice.Payments;
using static TransactionsMicroservice.Transactions;

namespace ReportsBranchMicroservice
{
    public class DataFetcher
    {
        private readonly TransactionsClient transactionsClient;
        private readonly AccountsClient accountsClient;
        private readonly PaymentsClient paymentsClient;
        private readonly CardsClient cardsClient;
        private readonly LoansClient loansClient;

        public DataFetcher(TransactionsClient transactionsClient,
         AccountsClient accountsClient,
         PaymentsClient paymentsClient,
         CardsClient cardsClient,
         LoansClient loansClient)
        {
            this.transactionsClient = transactionsClient;
            this.accountsClient = accountsClient;
            this.paymentsClient = paymentsClient;
            this.cardsClient = cardsClient;
            this.loansClient = loansClient;
        }

        public async Task<Account[]> GetAccounts(Metadata headers, string userId)
        {
            var request = new GetUserAccountsRequest { UserId = userId };
            var response = await accountsClient.GetUserAccountsAsync(request, headers);
            return response.Accounts.ToArray();
        }

        public async Task<PaymentsAndLoans> GetPaymentsWithLoans(Metadata headers, string[] accountsIds)
        {
            var request = new GetPaymentsRequest { Ids = { accountsIds } };
            var response = await paymentsClient.GetByAccountsAsync(request, headers);
            return new PaymentsAndLoans
            {
                Loans = response.Loans.ToArray(),
                Payments = response.Payments.ToArray()
            };
        }

        public async Task<Card[]> GetCards(Metadata headers, string[] accountsIds)
        {
            var request = new GetCardsRequest { Ids = { accountsIds } };
            var response = await cardsClient.GetByAccountsAsync(request, headers);
            return response.Cards.ToArray();
        }

        public async Task<Transaction[]> GetAccountsTransactions(Metadata headers, string[] accountsIds, Timestamp from, Timestamp to)
        {
            var request = new AccountsMicroservice.GetTransactionsRequest { Ids = { accountsIds }, TimestampFrom = from, TimestampTo = to };
            var response = await accountsClient.GetTransactionsAsync(request, headers);
            return response.Transactions.ToArray();
        }

        public async Task<Transaction[]> GetCardsTransactions(Metadata headers, Timestamp from, Timestamp to)
        {
            var request = new CardsMicroservice.GetTransactionsRequest { TimestampFrom = from, TimestampTo = to };
            var response = await cardsClient.GetTransactionsAsync(request, headers);
            return response.Transactions.ToArray();
        }

        public async Task<Transaction[]> GetLoansTransactions(Metadata headers, Timestamp from, Timestamp to)
        {
            var request = new LoansMicroservice.GetTransactionsRequest { TimestampFrom = from, TimestampTo = to };
            var response = await loansClient.GetTransactionsAsync(request, headers);
            return response.Transactions.ToArray();
        }

        public async Task<Transaction[]> GetPaymentsTransactions(Metadata headers, Timestamp from, Timestamp to)
        {
            var request = new PaymentsMicroservice.GetTransactionsRequest { TimestampFrom = from, TimestampTo = to };
            var response = await paymentsClient.GetTransactionsAsync(request, headers);
            return response.Transactions.ToArray();
        }

        public async Task<Transaction[]> GetTransactions(Metadata headers, Timestamp from, Timestamp to)
        {
            var request = new TransactionsMicroservice.FilterTransactionsRequest { TimestampFrom = from, TimestampTo = to };
            var response = await transactionsClient.FilterAsync(request, headers);
            return response.Transactions.ToArray();
        }
    }
}
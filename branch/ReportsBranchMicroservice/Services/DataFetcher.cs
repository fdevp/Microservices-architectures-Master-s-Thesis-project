using System;
using System.Linq;
using System.Threading.Tasks;
using AccountsMicroservice;
using CardsMicroservice;
using Google.Protobuf.WellKnownTypes;
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

        public async Task<Account[]> GetAccounts(string flowId, string userId)
        {
            var request = new GetUserAccountsRequest { FlowId = flowId, UserId = userId };
            var response = await accountsClient.GetUserAccountsAsync(request);
            return response.Accounts.ToArray();
        }

        public async Task<PaymentsAndLoans> GetPaymentsWithLoans(string flowId, string[] accountsIds)
        {
            var request = new GetPaymentsRequest { FlowId = flowId, Ids = { accountsIds } };
            var response = await paymentsClient.GetByAccountsAsync(request);
            return new PaymentsAndLoans
            {
                Loans = response.Loans.ToArray(),
                Payments = response.Payments.ToArray()
            };
        }

        public async Task<Card[]> GetCards(string flowId, string[] accountsIds)
        {
            var request = new GetCardsRequest { FlowId = flowId, Ids = { accountsIds } };
            var response = await cardsClient.GetByAccountsAsync(request);
            return response.Cards.ToArray();
        }

        public async Task<Transaction[]> GetAccountsTransactions(string flowId, string[] accountsIds, Timestamp from, Timestamp to)
        {
            var request = new AccountsMicroservice.GetTransactionsRequest { FlowId = flowId, Ids = { accountsIds }, TimestampFrom = from, TimestampTo = to };
            var response = await accountsClient.GetTransactionsAsync(request);
            return response.Transactions.ToArray();
        }

        public async Task<Transaction[]> GetCardsTransactions(string flowId, Timestamp from, Timestamp to)
        {
            var request = new CardsMicroservice.GetTransactionsRequest { FlowId = flowId, TimestampFrom = from, TimestampTo = to };
            var response = await cardsClient.GetTransactionsAsync(request);
            return response.Transactions.ToArray();
        }

        public async Task<Transaction[]> GetLoansTransactions(string flowId, Timestamp from, Timestamp to)
        {
            var request = new LoansMicroservice.GetTransactionsRequest { FlowId = flowId, TimestampFrom = from, TimestampTo = to };
            var response = await loansClient.GetTransactionsAsync(request);
            return response.Transactions.ToArray();
        }

        public async Task<Transaction[]> GetPaymentsTransactions(string flowId, Timestamp from, Timestamp to)
        {
            var request = new PaymentsMicroservice.GetTransactionsRequest { FlowId = flowId, TimestampFrom = from, TimestampTo = to };
            var response = await paymentsClient.GetTransactionsAsync(request);
            return response.Transactions.ToArray();
        }

        public async Task<Transaction[]> GetTransactions(string flowId, Timestamp from, Timestamp to)
        {
            var request = new TransactionsMicroservice.FilterTransactionsRequest { FlowId = flowId, TimestampFrom = from, TimestampTo = to };
            var response = await transactionsClient.FilterAsync(request);
            return response.Transactions.ToArray();
        }
    }
}
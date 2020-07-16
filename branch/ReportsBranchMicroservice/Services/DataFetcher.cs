using System;
using System.Linq;
using System.Threading.Tasks;
using AccountsMicroservice;
using CardsMicroservice;
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

        public async Task<Account[]> GetAccounts(long flowId, string userId)
        {
            var request = new GetUserAccountsRequest { FlowId = flowId, UserId = userId };
            var response = await accountsClient.GetUserAccountsAsync(request);
            return response.Accounts.ToArray();
        }

        public async Task<PaymentsAndLoans> GetPaymentsWithLoans(long flowId, string[] accountsIds)
        {
            var request = new GetPaymentsRequest { FlowId = flowId, Ids = { accountsIds } };
            var response = await paymentsClient.GetByAccountsAsync(request);
            return new PaymentsAndLoans
            {
                Loans = response.Loans.ToArray(),
                Payments = response.Payments.ToArray()
            };
        }

        public async Task<Card[]> GetCards(long flowId, string[] accountsIds)
        {
            var request = new GetCardsRequest { FlowId = flowId, Ids = { accountsIds } };
            var response = await cardsClient.GetByAccountsAsync(request);
            return response.Cards.ToArray();
        }

        public async Task<Transaction[]> GetAccountsTransactions(long flowId, string[] accountsIds, long from, long to)
        {
            var request = new AccountsMicroservice.GetTransactionsRequest { FlowId = flowId, Ids = { accountsIds }, TimestampFrom = from, TimestampTo = to };
            var response = await accountsClient.GetTransactionsAsync(request);
            return response.Transactions.ToArray();
        }

        public async Task<Transaction[]> GetCardsTransactions(long flowId, long from, long to)
        {
            var request = new CardsMicroservice.GetTransactionsRequest { FlowId = flowId, TimestampFrom = from, TimestampTo = to };
            var response = await cardsClient.GetTransactionsAsync(request);
            return response.Transactions.ToArray();
        }

        public async Task<Transaction[]> GetLoansTransactions(long flowId, long from, long to)
        {
            var request = new LoansMicroservice.GetTransactionsRequest { FlowId = flowId, TimestampFrom = from, TimestampTo = to };
            var response = await loansClient.GetTransactionsAsync(request);
            return response.Transactions.ToArray();
        }

        public async Task<Transaction[]> GetPaymentsTransactions(long flowId, long from, long to)
        {
            var request = new PaymentsMicroservice.GetTransactionsRequest { FlowId = flowId, TimestampFrom = from, TimestampTo = to };
            var response = await paymentsClient.GetTransactionsAsync(request);
            return response.Transactions.ToArray();
        }

        public async Task<Transaction[]> GetTransactions(long flowId, long from, long to)
        {
            var request = new TransactionsMicroservice.FilterTransactionsRequest { FlowId = flowId, TimestampFrom = from, TimestampTo = to };
            var response = await transactionsClient.FilterAsync(request);
            return response.Transactions.ToArray();
        }
    }
}
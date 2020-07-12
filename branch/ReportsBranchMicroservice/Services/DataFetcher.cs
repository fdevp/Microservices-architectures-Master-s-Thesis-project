using System;
using System.Linq;
using System.Threading.Tasks;
using AccountsMicroservice;
using CardsMicroservice;
using PaymentsMicroservice;
using static AccountsMicroservice.Accounts;
using static CardsMicroservice.Cards;
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

        public DataFetcher(TransactionsClient transactionsClient,
         AccountsClient accountsClient,
         PaymentsClient paymentsClient,
         CardsClient cardsClient)
        {
            this.transactionsClient = transactionsClient;
            this.accountsClient = accountsClient;
            this.paymentsClient = paymentsClient;
            this.cardsClient = cardsClient;
        }

        public async Task<Account[]> GetAccounts(long flowId, string userId)
        {
            var request = new GetUserAccountsRequest { FlowId = flowId, UserId = userId };
            var response = await accountsClient.GetUserAccountsAsync(request);
            return response.Accounts.ToArray();
        }

        public async Task<PaymentsAndLoans> GetPaymentsWithLoans(long flowId, string[] accountsIds)
        {
            var request = new GetByAccountRequest { FlowId = flowId, AccountIds = { accountsIds } };
            var response = await paymentsClient.GetByAccountAsync(request);
            return new PaymentsAndLoans
            {
                Loans = response.Loans.ToArray(),
                Payments = response.Payments.ToArray()
            };
        }

        public async Task<Transaction[]> GetTransactions(long flowId, string[] accountsIds)
        {
            var request = new AccountsMicroservice.GetTransactionsRequest { FlowId = flowId, Ids = { accountsIds } };
            var response = await accountsClient.GetTransactionsAsync(request);
            return response.Transactions.ToArray();
        }

        public async Task<Card[]> GetCards(long flowId, string[] accountsIds)
        {
            var request = new GetCardsByAccountsRequest { FlowId = flowId, AccountIds = { accountsIds } };
            var response = await cardsClient.GetByAccountsAsync(request);
            return response.Cards.ToArray();
        }
    }
}
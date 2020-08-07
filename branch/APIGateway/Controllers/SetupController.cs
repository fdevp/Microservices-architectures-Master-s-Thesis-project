using System.Linq;
using System.Threading.Tasks;
using AccountsMicroservice;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using static AccountsMicroservice.Accounts;
using static CardsMicroservice.Cards;
using static LoansMicroservice.Loans;
using static PaymentsMicroservice.Payments;
using static TransactionsMicroservice.Transactions;
using static UsersMicroservice.Users;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SetupController : ControllerBase
    {
        private readonly AccountsClient accountsClient;
        private readonly CardsClient cardsClient;
        private readonly LoansClient loansClient;
        private readonly PaymentsClient paymentsClient;
        private readonly TransactionsClient transactionsClient;
        private readonly UsersClient usersClient;
        private readonly Mapper mapper;

        public SetupController(AccountsClient accountsClient,
         CardsClient cardsClient,
         LoansClient loansClient,
         PaymentsClient paymentsClient,
         TransactionsClient transactionsClient,
         UsersClient usersClient,
         Mapper mapper)
        {
            this.accountsClient = accountsClient;
            this.cardsClient = cardsClient;
            this.loansClient = loansClient;
            this.paymentsClient = paymentsClient;
            this.transactionsClient = transactionsClient;
            this.usersClient = usersClient;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("clear")]
        public async Task Clear()
        {
            await usersClient.SetupAsync(new UsersMicroservice.SetupRequest());
            await accountsClient.SetupAsync(new AccountsMicroservice.SetupRequest());
            await cardsClient.SetupAsync(new CardsMicroservice.SetupRequest());
            await loansClient.SetupAsync(new LoansMicroservice.SetupRequest());
            await paymentsClient.SetupAsync(new PaymentsMicroservice.SetupRequest());
        }

        [HttpPost]
        [Route("setup")]
        public async Task Setup(SetupAll setup)
        {
            var usersRequest = mapper.Map<UsersMicroservice.SetupRequest>(setup.UsersSetup);
            await usersClient.SetupAsync(usersRequest);

            var accountsRequest = mapper.Map<AccountsMicroservice.SetupRequest>(setup.AccountsSetup);
            await accountsClient.SetupAsync(accountsRequest);

            var cardsRequest = mapper.Map<CardsMicroservice.SetupRequest>(setup.CardsSetup);
            await cardsClient.SetupAsync(cardsRequest);

            for (int i = 0; i < setup.LoansSetup.loans.Length; i += 10000)
            {
                var portion = setup.LoansSetup.loans.Skip(i).Take(10000).ToArray();
                var request = mapper.Map<LoansMicroservice.SetupRequest>(new LoansSetup { loans = portion });
                await loansClient.SetupAppendAsync(request);
            }

            for (int i = 0; i < setup.PaymentsSetup.payments.Length; i += 10000)
            {
                var portion = setup.PaymentsSetup.payments.Skip(i).Take(10000).ToArray();
                var request = mapper.Map<PaymentsMicroservice.SetupRequest>(new PaymentsSetup { payments = portion });
                await paymentsClient.SetupAppendAsync(request);
            }

            for (int i = 0; i < setup.TransactionsSetup.transactions.Length; i += 10000)
            {
                var portion = setup.TransactionsSetup.transactions.Skip(i).Take(10000).ToArray();
                var request = mapper.Map<TransactionsMicroservice.SetupRequest>(new TransactionsSetup { transactions = portion });
                await transactionsClient.SetupAppendAsync(request);
            }
        }
    }
}
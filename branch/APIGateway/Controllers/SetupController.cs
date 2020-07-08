using System.Threading.Tasks;
using AccountsMicroservice;
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
        [Route("setup")]
        public async Task Setup(SetupAll setup)
        {
            var usersRequest = mapper.Map<UsersMicroservice.SetupRequest>(setup.UsersSetup);
            await usersClient.SetupAsync(usersRequest);

            var accountsRequest = mapper.Map<AccountsMicroservice.SetupRequest>(setup.AccountsSetup);
            await accountsClient.SetupAsync(accountsRequest);

            var cardsRequest = mapper.Map<CardsMicroservice.SetupRequest>(setup.CardsSetup);
            await cardsClient.SetupAsync(cardsRequest);

            var loansRequest = mapper.Map<LoansMicroservice.SetupRequest>(setup.LoansSetup);
            await loansClient.SetupAsync(loansRequest);

            var paymentsRequest = mapper.Map<PaymentsMicroservice.SetupRequest>(setup.PaymentsSetup);
            await paymentsClient.SetupAsync(paymentsRequest);

            var transactionsRequest = mapper.Map<TransactionsMicroservice.SetupRequest>(setup.TransactionsSetup);
            await transactionsClient.SetupAsync(transactionsRequest);
        }
    }
}
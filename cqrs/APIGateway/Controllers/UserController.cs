using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountsReadMicroservice;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using CardsReadMicroservice;
using Google.Protobuf.Collections;
using LoansReadMicroservice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentsReadMicroservice;
using TransactionsReadMicroservice;
using UsersMicroservice;
using static AccountsReadMicroservice.AccountsRead;
using static CardsReadMicroservice.CardsRead;
using static LoansReadMicroservice.LoansRead;
using static PaymentsReadMicroservice.PaymentsRead;
using static TransactionsReadMicroservice.TransactionsRead;
using static UsersMicroservice.Users;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> logger;
        private readonly UsersClient usersClient;
        private readonly AccountsReadClient accountsReadClient;
        private readonly TransactionsReadClient transactionsReadClient;
        private readonly PaymentsReadClient paymentsReadClient;
        private readonly LoansReadClient loansReadClient;
        private readonly CardsReadClient cardsReadClient;
        private readonly Mapper mapper;

        public UserController(ILogger<UserController> logger,
         UsersClient usersClient,
         AccountsReadClient accountsReadClient,
         TransactionsReadClient transactionsReadClient,
         PaymentsReadClient paymentsReadClient,
         LoansReadClient loansReadClient,
         CardsReadClient cardsReadClient,
         Mapper mapper)
        {
            this.logger = logger;
            this.usersClient = usersClient;
            this.accountsReadClient = accountsReadClient;
            this.transactionsReadClient = transactionsReadClient;
            this.paymentsReadClient = paymentsReadClient;
            this.loansReadClient = loansReadClient;
            this.cardsReadClient = cardsReadClient;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("{userId}/panel")]
        public async Task<Panel> Panel(string userId)
        {
            var flowId = (long)HttpContext.Items["flowId"];

            RepeatedField<Loan> loans = new RepeatedField<Loan>();
            RepeatedField<Payment> payments = new RepeatedField<Payment>();
            RepeatedField<Account> accounts = new RepeatedField<Account>();
            RepeatedField<Card> cards = new RepeatedField<Card>();
            RepeatedField<Transaction> transactions = new RepeatedField<Transaction>();

            var accountsResponse = await accountsReadClient.GetUserAccountsAsync(new GetUserAccountsRequest { FlowId = flowId, UserId = userId });
            var accountsIds = accountsResponse.Accounts.Select(a => a.Id).ToArray();
            accounts = accountsResponse.Accounts;


            var parallelTasks = new List<Task>();
            parallelTasks.Add(Task.Run(async () =>
            {
                var transactionsResponse = await transactionsReadClient.FilterAsync(new FilterTransactionsRequest { FlowId = flowId, Senders = { accountsIds } });
                transactions = transactionsResponse.Transactions;
            }));

            parallelTasks.Add(Task.Run(async () =>
            {
                var paymentsResponse = await paymentsReadClient.GetByAccountsAsync(new GetPaymentsRequest { FlowId = flowId, Ids = { accountsIds } });
                payments = paymentsResponse.Payments;

                var paymentsIds = payments.Select(p => p.Id);
                var loansResponse = await loansReadClient.GetLoansByPaymentsAsync(new GetLoansByPaymentsRequest { FlowId = flowId, PaymentsIds = { paymentsIds } });
                loans = loansResponse.Loans;
            }));

            parallelTasks.Add(Task.Run(async () =>
            {
                var cardsResponse = await cardsReadClient.GetByAccountsAsync(new GetCardsByAccountsRequest { FlowId = flowId, AccountIds = { accountsIds } });
                cards = cardsResponse.Cards;
            }));


            await Task.WhenAll(parallelTasks);
            return new Panel
            {
                Accounts = mapper.Map<AccountDTO[]>(accounts),
                Loans = mapper.Map<LoanDTO[]>(loans),
                Payments = mapper.Map<PaymentDTO[]>(payments),
                Transactions = mapper.Map<TransactionDTO[]>(transactions),
                Cards = mapper.Map<CardDTO[]>(cards),
            };
        }

        [HttpPost]
        [Route("token")]
        public async Task<string> Token(TokenRequest data)
        {
            var request = mapper.Map<SignInRequest>(data);
            request.FlowId = (long)HttpContext.Items["flowId"];
            var response = await usersClient.TokenAsync(request);
            return response.Token;
        }

        [HttpPost]
        [Route("logout")]
        public async Task Logout(Models.LogoutRequest data)
        {
            var request = new UsersMicroservice.LogoutRequest { FlowId = (long)HttpContext.Items["flowId"], Token = data.Token };
            await usersClient.LogoutAsync(request);
        }

        [HttpPost]
        [Route("setup")]
        public async Task Setup(UsersSetup setup)
        {
            var request = mapper.Map<SetupRequest>(setup);
            await usersClient.SetupAsync(request);
        }
    }
}
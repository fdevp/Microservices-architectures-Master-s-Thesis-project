using System.Threading.Tasks;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using static AccountsWriteMicroservice.AccountsWrite;
using static CardsWriteMicroservice.CardsWrite;
using static LoansWriteMicroservice.LoansWrite;
using static PaymentsWriteMicroservice.PaymentsWrite;
using static TransactionsWriteMicroservice.TransactionsWrite;
using static UsersMicroservice.Users;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SetupController : ControllerBase
    {
        private readonly AccountsWriteClient accountsClient;
        private readonly CardsWriteClient cardsClient;
        private readonly LoansWriteClient loansClient;
        private readonly PaymentsWriteClient paymentsClient;
        private readonly TransactionsWriteClient transactionsClient;
        private readonly UsersClient usersClient;
        private readonly Mapper mapper;

        public SetupController(AccountsWriteClient accountsClient,
         CardsWriteClient cardsClient,
         LoansWriteClient loansClient,
         PaymentsWriteClient paymentsClient,
         TransactionsWriteClient transactionsClient,
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

            var accountsRequest = mapper.Map<AccountsWriteMicroservice.SetupRequest>(setup.AccountsSetup);
            await accountsClient.SetupAsync(accountsRequest);

            var cardsRequest = mapper.Map<CardsWriteMicroservice.SetupRequest>(setup.CardsSetup);
            await cardsClient.SetupAsync(cardsRequest);

            var loansRequest = mapper.Map<LoansWriteMicroservice.SetupRequest>(setup.LoansSetup);
            await loansClient.SetupAsync(loansRequest);

            var paymentsRequest = mapper.Map<PaymentsWriteMicroservice.SetupRequest>(setup.PaymentsSetup);
            await paymentsClient.SetupAsync(paymentsRequest);

            var transactionsRequest = mapper.Map<TransactionsWriteMicroservice.SetupRequest>(setup.TransactionsSetup);
            await transactionsClient.SetupAsync(transactionsRequest);
        }
    }
}

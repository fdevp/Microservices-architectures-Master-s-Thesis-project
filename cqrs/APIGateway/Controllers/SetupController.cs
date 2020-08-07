using System.Linq;
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
        [Route("clear")]
        public async Task Clear()
        {
            await usersClient.SetupAsync(new UsersMicroservice.SetupRequest());
            await accountsClient.SetupAsync(new AccountsWriteMicroservice.SetupRequest());
            await cardsClient.SetupAsync(new CardsWriteMicroservice.SetupRequest());
            await loansClient.SetupAsync(new LoansWriteMicroservice.SetupRequest());
            await paymentsClient.SetupAsync(new PaymentsWriteMicroservice.SetupRequest());
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

            for (int i = 0; i < setup.LoansSetup.loans.Length; i += 10000)
            {
                var portion = setup.LoansSetup.loans.Skip(i).Take(10000).ToArray();
                var request = mapper.Map<LoansWriteMicroservice.SetupRequest>(new LoansSetup { loans = portion });
                await loansClient.SetupAppendAsync(request);
            }

            for (int i = 0; i < setup.PaymentsSetup.payments.Length; i += 10000)
            {
                var portion = setup.PaymentsSetup.payments.Skip(i).Take(10000).ToArray();
                var request = mapper.Map<PaymentsWriteMicroservice.SetupRequest>(new PaymentsSetup { payments = portion });
                await paymentsClient.SetupAppendAsync(request);
            }

            for (int i = 0; i < setup.TransactionsSetup.transactions.Length; i += 10000)
            {
                var portion = setup.TransactionsSetup.transactions.Skip(i).Take(10000).ToArray();
                var request = mapper.Map<TransactionsWriteMicroservice.SetupRequest>(new TransactionsSetup { transactions = portion });
                await transactionsClient.SetupAppendAsync(request);
            }
        }
    }
}

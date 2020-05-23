using System.Threading.Tasks;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TransactionsMicroservice;
using static TransactionsMicroservice.Transactions;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ILogger<TransactionController> logger;
        private readonly TransactionsClient transactionsClient;
        private readonly Mapper mapper;

        public TransactionController(ILogger<TransactionController> logger, TransactionsClient transactionsClient, Mapper mapper)
        {
            this.logger = logger;
            this.transactionsClient = transactionsClient;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("setup")]
        public async Task Setup(TransactionsSetup setup)
        {
            var request = mapper.Map<SetupRequest>(setup);
            await transactionsClient.SetupAsync(request);
        }

        [HttpPost]
        [Route("teardown")]
        public async Task TearDown()
        {
            await transactionsClient.TearDownAsync(new Empty());
        }
    }
}
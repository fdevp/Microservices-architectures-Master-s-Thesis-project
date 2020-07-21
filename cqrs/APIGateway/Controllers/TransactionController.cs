using System.Linq;
using System.Threading.Tasks;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static TransactionsWriteMicroservice.TransactionsWrite;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ILogger<TransactionController> logger;
        private readonly TransactionsWriteClient transactionsWriteClient;
        private readonly Mapper mapper;

        public TransactionController(ILogger<TransactionController> logger, TransactionsWriteClient transactionsWriteClient, Mapper mapper)
        {
            this.logger = logger;
            this.transactionsWriteClient = transactionsWriteClient;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("setup")]
        public async Task Setup(TransactionsSetup setup)
        {
             for (int i = 0; i < setup.transactions.Length; i += 10000)
            {
                var portion = setup.transactions.Skip(i).Take(10000).ToArray();
                var request = mapper.Map<TransactionsWriteMicroservice.SetupRequest>(new TransactionsSetup { transactions = portion });
                await transactionsWriteClient.SetupAppendAsync(request);
            }
        }
    }
}
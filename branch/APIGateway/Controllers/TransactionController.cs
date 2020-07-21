using System.Linq;
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
            for (int i = 0; i < setup.transactions.Length; i += 10000)
            {
                var portion = setup.transactions.Skip(i).Take(10000).ToArray();
                var request = mapper.Map<TransactionsMicroservice.SetupRequest>(new TransactionsSetup { transactions = portion });
                await transactionsClient.SetupAppendAsync(request);
            }
        }
    }
}
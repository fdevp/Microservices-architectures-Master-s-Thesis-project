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
            var request = mapper.Map<TransactionsWriteMicroservice.SetupRequest>(setup);
            await transactionsWriteClient.SetupAsync(request);
        }
    }
}
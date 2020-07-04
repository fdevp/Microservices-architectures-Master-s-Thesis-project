using System.Threading.Tasks;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedClasses.Events.Transactions;
using SharedClasses.Messaging;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ILogger<TransactionController> logger;
        private readonly PublishingRouter publishingRouter;
        private readonly Mapper mapper;

        public TransactionController(ILogger<TransactionController> logger, PublishingRouter publishingRouter, Mapper mapper)
        {
            this.logger = logger;
            this.publishingRouter = publishingRouter;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("setup")]
        public Task Setup(TransactionsSetup setup)
        {
            var payload = mapper.Map<SetupTransactionsEvent>(setup);
            publishingRouter.Publish(Queues.Accounts, payload, null);
            return Task.CompletedTask;
        }
    }
}
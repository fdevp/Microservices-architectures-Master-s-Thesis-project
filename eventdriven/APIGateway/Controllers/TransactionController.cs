using System.Linq;
using System.Threading.Tasks;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedClasses.Events.Transactions;
using SharedClasses.Messaging;
using SharedClasses.Models;

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
            for (int i = 0; i < setup.transactions.Length; i += 10000)
            {
                var portion = setup.transactions.Skip(i).Take(10000).ToArray();
                var transactionsEvent = new SetupAppendTransactionsEvent { Transactions = portion.Select(t => mapper.Map<Transaction>(t)).ToArray() };
                this.publishingRouter.Publish(Queues.Transactions, transactionsEvent, null);
            }
            return Task.CompletedTask;
        }
    }
}
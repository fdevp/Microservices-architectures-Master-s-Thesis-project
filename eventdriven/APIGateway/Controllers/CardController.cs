using System.Linq;
using System.Threading.Tasks;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedClasses.Events.Cards;
using SharedClasses.Events.Transactions;
using SharedClasses.Messaging;
using SharedClasses.Models;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CardController : ControllerBase
    {
        private readonly ILogger<CardController> logger;
        private readonly Mapper mapper;
        private readonly PublishingRouter publishingRouter;
        private readonly EventsAwaiter eventsAwaiter;

        public CardController(ILogger<CardController> logger, Mapper mapper, PublishingRouter publishingRouter, EventsAwaiter eventsAwaiter)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.publishingRouter = publishingRouter;
            this.eventsAwaiter = eventsAwaiter;
        }

        [HttpPost]
        [Route("transfer")]
        public async Task<TransactionDTO> Transfer(CardTransfer data)
        {
            var transferEvent = mapper.Map<CardTransferEvent>(data);
            var flowId = HttpContext.Items["flowId"].ToString();
            var response = await eventsAwaiter.AwaitResponse<SelectedTransactionsEvent>(flowId, () => publishingRouter.Publish(Queues.Cards, transferEvent, flowId, Queues.APIGateway));
            var transaction = response.Transactions.Single();
            return mapper.Map<TransactionDTO>(transaction);
        }


        [HttpPost]
        [Route("setup")]
        public Task Setup(CardsSetup setup)
        {
            var cardsEvent = mapper.Map<SetupCardsEvent>(setup);
            this.publishingRouter.Publish(Queues.Cards, cardsEvent, null);
            return Task.CompletedTask;
        }
    }
}
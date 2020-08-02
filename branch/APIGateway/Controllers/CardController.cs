using System.Threading.Tasks;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using CardsMicroservice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static CardsMicroservice.Cards;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CardController : ControllerBase
    {
        private readonly ILogger<CardController> logger;
        private readonly CardsClient cardsClient;
        private readonly Mapper mapper;

        public CardController(ILogger<CardController> logger, CardsClient cardsClient, Mapper mapper)
        {
            this.logger = logger;
            this.cardsClient = cardsClient;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("transfer")]
        public async Task<TransactionDTO> Transfer(CardTransfer data)
        {
            var request = mapper.Map<TransferRequest>(data);
            request.FlowId = HttpContext.Items["flowId"].ToString();
            var response = await cardsClient.TransferAsync(request);
            var transaction = mapper.Map<TransactionDTO>(response.Transaction);
            return transaction;
        }


        [HttpPost]
        [Route("setup")]
        public async Task Setup(CardsSetup setup)
        {
            var request = mapper.Map<SetupRequest>(setup);
            await cardsClient.SetupAsync(request);
        }
    }
}
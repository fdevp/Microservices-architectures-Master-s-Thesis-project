using System.Threading.Tasks;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using CardsWriteMicroservice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static CardsReadMicroservice.CardsRead;
using static CardsWriteMicroservice.CardsWrite;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CardController : ControllerBase
    {
        private readonly ILogger<CardController> logger;
        private readonly CardsWriteClient cardsWriteClient;
        private readonly Mapper mapper;

        public CardController(ILogger<CardController> logger, CardsWriteClient cardsWriteClient, Mapper mapper)
        {
            this.logger = logger;
            this.cardsWriteClient = cardsWriteClient;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("transfer")]
        public async Task Transfer(CardTransfer data)
        {
            var request = mapper.Map<TransferRequest>(data);
            request.FlowId = HttpContext.Items["flowId"].ToString();
            await cardsWriteClient.TransferAsync(request);
        }


        [HttpPost]
        [Route("setup")]
        public async Task Setup(CardsSetup setup)
        {
            var request = mapper.Map<SetupRequest>(setup);
            await cardsWriteClient.SetupAsync(request);
        }
    }
}
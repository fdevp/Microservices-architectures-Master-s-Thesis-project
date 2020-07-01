using System.Threading.Tasks;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace APIGateway.Controllers
{
    // [ApiController]
    // [Route("[controller]")]
    // public class CardController : ControllerBase
    // {
    //     private readonly ILogger<CardController> logger;
    //     private readonly Mapper mapper;

    //     public CardController(ILogger<CardController> logger, Mapper mapper)
    //     {
    //         this.logger = logger;
    //         this.mapper = mapper;
    //     }

    //     [HttpPost]
    //     [Route("transfer")]
    //     public async Task<TransactionDTO> Transfer(CardTransfer data)
    //     {
    //         var request = mapper.Map<TransferRequest>(data);
    //         request.FlowId = (long)HttpContext.Items["flowId"];
    //         var response = await cardsWriteClient.TransferAsync(request);
    //         var transaction = mapper.Map<TransactionDTO>(response.Transaction);
    //         return transaction;
    //     }


    //     [HttpPost]
    //     [Route("setup")]
    //     public async Task Setup(CardsSetup setup)
    //     {
    //         var request = mapper.Map<SetupRequest>(setup);
    //         await cardsWriteClient.SetupAsync(request);
    //     }
    // }
}
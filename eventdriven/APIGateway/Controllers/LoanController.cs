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
    // public class LoanController : ControllerBase
    // {
    //     private readonly ILogger<LoanController> logger;
    //     private readonly Mapper mapper;

    //     public LoanController(ILogger<LoanController> logger, Mapper mapper)
    //     {
    //         this.logger = logger;
    //         this.mapper = mapper;
    //     }

    //     [HttpPost]
    //     [Route("repay")]
    //     public async Task Repay(BatchInstalments data)
    //     {
    //         var request = new BatchRepayInstalmentsRequest { FlowId = (long)HttpContext.Items["flowId"], Ids = { data.RepaidInstalmentsIds } };
    //         await loansWriteClient.BatchRepayInstalmentsAsync(request);
    //     }

    //     [HttpPost]
    //     [Route("setup")]
    //     public async Task Setup(LoansSetup setup)
    //     {
    //         var request = mapper.Map<SetupRequest>(setup);
    //         await loansWriteClient.SetupAsync(request);
    //     }
    // }
}

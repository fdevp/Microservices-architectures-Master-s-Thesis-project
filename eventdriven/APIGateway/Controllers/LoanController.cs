using System.Threading.Tasks;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using LoansWriteMicroservice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static LoansWriteMicroservice.LoansWrite;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoanController : ControllerBase
    {
        private readonly ILogger<LoanController> logger;
        private readonly LoansWriteClient loansWriteClient;
        private readonly Mapper mapper;

        public LoanController(ILogger<LoanController> logger, LoansWriteClient loansWriteClient, Mapper mapper)
        {
            this.logger = logger;
            this.loansWriteClient = loansWriteClient;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("repay")]
        public async Task Repay(BatchInstalments data)
        {
            var request = new BatchRepayInstalmentsRequest { FlowId = (long)HttpContext.Items["flowId"], Ids = { data.RepaidInstalmentsIds } };
            await loansWriteClient.BatchRepayInstalmentsAsync(request);
        }

        [HttpPost]
        [Route("setup")]
        public async Task Setup(LoansSetup setup)
        {
            var request = mapper.Map<SetupRequest>(setup);
            await loansWriteClient.SetupAsync(request);
        }
    }
}

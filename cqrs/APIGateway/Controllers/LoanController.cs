using System.Linq;
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
        [Route("setup")]
        public async Task Setup(LoansSetup setup)
        {
            for (int i = 0; i < setup.loans.Length; i += 10000)
            {
                var portion = setup.loans.Skip(i).Take(10000).ToArray();
                var request = mapper.Map<LoansWriteMicroservice.SetupRequest>(new LoansSetup { loans = portion });
                await loansWriteClient.SetupAppendAsync(request);
            }
        }
    }
}

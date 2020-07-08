using System.Threading.Tasks;
using APIGateway.Models.Setup;
using AutoMapper;
using LoansMicroservice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static LoansMicroservice.Loans;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoanController : ControllerBase
    {
        private readonly ILogger<LoanController> logger;
        private readonly LoansClient loansClient;
        private readonly Mapper mapper;

        public LoanController(ILogger<LoanController> logger, LoansClient loansClient, Mapper mapper)
        {
            this.logger = logger;
            this.loansClient = loansClient;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("setup")]
        public async Task Setup(LoansSetup setup)
        {
            var request = mapper.Map<SetupRequest>(setup);
            await loansClient.SetupAsync(request);
        }
    }
}
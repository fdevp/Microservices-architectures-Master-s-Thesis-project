using System.Linq;
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
            for (int i = 0; i < setup.loans.Length; i += 10000)
            {
                var portion = setup.loans.Skip(i).Take(10000).ToArray();
                var request = mapper.Map<LoansMicroservice.SetupRequest>(new LoansSetup { loans = portion });
                await loansClient.SetupAppendAsync(request);
            }
        }
    }
}
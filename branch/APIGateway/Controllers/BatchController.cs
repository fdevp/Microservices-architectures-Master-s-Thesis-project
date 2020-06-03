using System.Linq;
using System.Threading.Tasks;
using APIGateway.Models;
using AutoMapper;
using BatchesBranchMicroservice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static BatchesBranchMicroservice.BatchesBranch;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BatchController : ControllerBase
    {
        private readonly ILogger<BatchController> logger;
        private readonly Mapper mapper;
        private readonly BatchesBranchClient batchesBranchClient;

        public BatchController(ILogger<BatchController> logger, Mapper mapper, BatchesBranchClient batchesBranchClient)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.batchesBranchClient = batchesBranchClient;
        }

        [HttpGet]
        public async Task<BatchData> Get([FromQuery(Name = "part")] int part, [FromQuery(Name = "total")] int total)
        {
            var request = new GetDataToProcessRequest { Part = part, TotalParts = total };
            request.FlowId = (long)HttpContext.Items["flowId"];
            var response = await batchesBranchClient.GetAsync(request);

            var balances = response.Balances.Select(b => mapper.Map<BalanceDTO>(b)).ToArray();
            var loans = response.Loans.Select(l => mapper.Map<LoanDTO>(l)).ToArray();
            var payments = response.Payments.Select(p => mapper.Map<PaymentDTO>(p)).ToArray();

            return new BatchData
            {
                Balances = balances,
                Loans = loans,
                Payments = payments
            };
        }
    }
}
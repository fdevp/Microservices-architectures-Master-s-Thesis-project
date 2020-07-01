using System.Linq;
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
    // public class PaymentController : ControllerBase
    // {
    //     private readonly ILogger<PaymentController> logger;
    //     private readonly Mapper mapper;

    //     public PaymentController(ILogger<PaymentController> logger, Mapper mapper)
    //     {
    //         this.logger = logger;
    //         this.mapper = mapper;
    //     }

    //     [HttpGet]
    //     [Route("loans")]
    //     public async Task<PaymentsLoans> Loans([FromQuery(Name = "part")] int part, [FromQuery(Name = "total")] int total)
    //     {
    //         var flowId = (long)HttpContext.Items["flowId"];
    //         var paymentsAndLoansRequest = new GetPaymentsWithLoansRequest { FlowId = flowId, Part = part, TotalParts = total };
    //         var paymentsAndLoans = await paymentsReadClient.GetWithLoansAsync(paymentsAndLoansRequest);

    //         return new PaymentsLoans
    //         {
    //             Loans = paymentsAndLoans.Loans.Select(l => mapper.Map<LoanDTO>(l)).ToArray(),
    //             Payments = paymentsAndLoans.Payments.Select(p => mapper.Map<PaymentDTO>(p)).ToArray()
    //         };
    //     }

    //     [HttpPost]
    //     [Route("setup")]
    //     public async Task Setup(PaymentsSetup setup)
    //     {
    //         var request = mapper.Map<PaymentsWriteMicroservice.SetupRequest>(setup);
    //         await paymentsWriteClient.SetupAsync(request);
    //     }
    // }
}
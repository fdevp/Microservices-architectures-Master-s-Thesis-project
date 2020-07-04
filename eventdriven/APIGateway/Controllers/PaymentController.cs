using System.Linq;
using System.Threading.Tasks;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedClasses.Events.Payments;
using SharedClasses.Messaging;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> logger;
        private readonly Mapper mapper;
        private readonly PublishingRouter publishingRouter;
        private readonly EventsAwaiter eventsAwaiter;

        public PaymentController(ILogger<PaymentController> logger, Mapper mapper, PublishingRouter publishingRouter, EventsAwaiter eventsAwaiter)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.publishingRouter = publishingRouter;
            this.eventsAwaiter = eventsAwaiter;
        }

        // [HttpGet]
        // [Route("loans")]
        // public async Task<PaymentsLoans> Loans([FromQuery(Name = "part")] int part, [FromQuery(Name = "total")] int total)
        // {
        //     var flowId = HttpContext.Items["flowId"].ToString();
        //     var getPaymentsEvent = new GetPaymentsEvent { FlowId = flowId, Part = part, TotalParts = total };
        //     var payments = await paymentsReadClient.GetWithLoansAsync(paymentsAndLoansRequest);

        //     return new PaymentsLoans
        //     {
        //         Loans = paymentsAndLoans.Loans.Select(l => mapper.Map<LoanDTO>(l)).ToArray(),
        //         Payments = paymentsAndLoans.Payments.Select(p => mapper.Map<PaymentDTO>(p)).ToArray()
        //     };
        // }

        [HttpPost]
        [Route("setup")]
        public Task Setup(PaymentsSetup setup)
        {
            var paymentsEvent = mapper.Map<SetupPaymentsEvent>(setup);
            this.publishingRouter.Publish(Queues.Payments, paymentsEvent, null);
            return Task.CompletedTask;
        }
    }
}
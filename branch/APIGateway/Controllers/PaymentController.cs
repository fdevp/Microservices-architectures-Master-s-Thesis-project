using System.Threading.Tasks;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentsMicroservice;
using static PaymentsMicroservice.Payments;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> logger;
        private readonly PaymentsClient paymentsClient;
        private readonly Mapper mapper;

        public PaymentController(ILogger<PaymentController> logger, PaymentsClient paymentsClient, Mapper mapper)
        {
            this.logger = logger;
            this.paymentsClient = paymentsClient;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("setup")]
        public async Task Setup(PaymentsSetup setup)
        {
            var request = mapper.Map<SetupRequest>(setup);
            await paymentsClient.SetupAsync(request);
        }
    }
}
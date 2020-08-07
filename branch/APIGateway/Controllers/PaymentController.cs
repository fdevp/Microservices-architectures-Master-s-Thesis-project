using System.Linq;
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
            for (int i = 0; i < setup.payments.Length; i += 10000)
            {
                var portion = setup.payments.Skip(i).Take(10000).ToArray();
                var request = mapper.Map<PaymentsMicroservice.SetupRequest>(new PaymentsSetup { payments = portion });
                await paymentsClient.SetupAppendAsync(request);
            }
        }
    }
}
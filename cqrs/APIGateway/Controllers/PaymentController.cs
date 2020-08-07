using System.Linq;
using System.Threading.Tasks;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PaymentsReadMicroservice;
using static PaymentsReadMicroservice.PaymentsRead;
using static PaymentsWriteMicroservice.PaymentsWrite;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> logger;
        private readonly PaymentsWriteClient paymentsWriteClient;
        private readonly Mapper mapper;

        public PaymentController(ILogger<PaymentController> logger, PaymentsWriteClient paymentsWriteClient, Mapper mapper)
        {
            this.logger = logger;
            this.paymentsWriteClient = paymentsWriteClient;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("setup")]
        public async Task Setup(PaymentsSetup setup)
        {
            for (int i = 0; i < setup.payments.Length; i += 10000)
            {
                var portion = setup.payments.Skip(i).Take(10000).ToArray();
                var request = mapper.Map<PaymentsWriteMicroservice.SetupRequest>(new PaymentsSetup { payments = portion });
                await paymentsWriteClient.SetupAppendAsync(request);
            }
        }
    }
}
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
        private readonly PaymentsReadClient paymentsReadClient;
        private readonly Mapper mapper;

        public PaymentController(ILogger<PaymentController> logger, PaymentsWriteClient paymentsWriteClient, PaymentsReadClient paymentsReadClient, Mapper mapper)
        {
            this.logger = logger;
            this.paymentsWriteClient = paymentsWriteClient;
            this.paymentsReadClient = paymentsReadClient;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("setup")]
        public async Task Setup(PaymentsSetup setup)
        {
            var request = mapper.Map<PaymentsWriteMicroservice.SetupRequest>(setup);
            await paymentsWriteClient.SetupAsync(request);
        }
    }
}
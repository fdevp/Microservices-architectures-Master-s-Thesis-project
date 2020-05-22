using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly ILogger<PaymentController> logger;

        public PaymentController(ILogger<PaymentController> logger)
        {
            this.logger = logger;
        }
    }
}
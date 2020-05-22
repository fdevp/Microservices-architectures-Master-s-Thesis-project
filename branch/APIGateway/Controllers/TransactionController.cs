using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ILogger<TransactionController> logger;

        public TransactionController(ILogger<TransactionController> logger)
        {
            this.logger = logger;
        }
    }
}
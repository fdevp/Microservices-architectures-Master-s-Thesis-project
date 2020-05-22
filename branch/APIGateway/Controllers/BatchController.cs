using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BatchController : ControllerBase
    {
        private readonly ILogger<BatchController> logger;

        public BatchController(ILogger<BatchController> logger)
        {
            this.logger = logger;
        }
    }
}
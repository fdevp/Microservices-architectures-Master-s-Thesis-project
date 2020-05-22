using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> logger;

        public ReportController(ILogger<ReportController> logger)
        {
            this.logger = logger;
        }
    }
}
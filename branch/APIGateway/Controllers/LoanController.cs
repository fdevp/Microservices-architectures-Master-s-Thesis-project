using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoanController : ControllerBase
    {
        private readonly ILogger<LoanController> logger;

        public LoanController(ILogger<LoanController> logger)
        {
            this.logger = logger;
        }
    }
}
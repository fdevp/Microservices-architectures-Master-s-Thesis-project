using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static ReportsBranchMicroservice.ReportsBranch;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> logger;

        public ReportController(ILogger<ReportController> logger, ReportsBranchClient reportsBranchClient)
        {
            this.logger = logger;
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static BatchesBranchMicroservice.BatchesBranch;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BatchController : ControllerBase
    {
        private readonly ILogger<BatchController> logger;

        public BatchController(ILogger<BatchController> logger, BatchesBranchClient batchesBranchClient)
        {
            this.logger = logger;
        }
    }
}
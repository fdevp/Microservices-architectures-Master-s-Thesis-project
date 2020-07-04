using System.Threading.Tasks;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedClasses.Events.Loans;
using SharedClasses.Messaging;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoanController : ControllerBase
    {
        private readonly ILogger<LoanController> logger;
        private readonly Mapper mapper;
        private readonly PublishingRouter publishingRouter;

        public LoanController(ILogger<LoanController> logger, Mapper mapper, PublishingRouter publishingRouter)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.publishingRouter = publishingRouter;
        }

        [HttpPost]
        [Route("setup")]
        public Task Setup(LoansSetup setup)
        {
            var loansEvent = mapper.Map<SetupLoansEvent>(setup);
            this.publishingRouter.Publish(Queues.Loans, loansEvent, null);
            return Task.CompletedTask;
        }
    }
}

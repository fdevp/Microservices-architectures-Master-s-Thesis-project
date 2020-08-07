using System.Linq;
using System.Threading.Tasks;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedClasses.Events.Loans;
using SharedClasses.Messaging;
using SharedClasses.Models;

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
            for (int i = 0; i < setup.loans.Length; i += 10000)
            {
                var portion = setup.loans.Skip(i).Take(10000).ToArray();
                var loansEvent = new SetupAppendLoansEvent { Loans = portion.Select(l => mapper.Map<Loan>(l)).ToArray() };
                this.publishingRouter.Publish(Queues.Loans, loansEvent, null);
            }
            return Task.CompletedTask;
        }
    }
}

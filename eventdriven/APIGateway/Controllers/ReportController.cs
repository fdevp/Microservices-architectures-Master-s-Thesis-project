using System.Threading.Tasks;
using APIGateway.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedClasses.Events.Reports;
using SharedClasses.Messaging;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> logger;
        private readonly EventsAwaiter eventsAwaiter;
        private readonly PublishingRouter publishingRouter;

        public ReportController(ILogger<ReportController> logger, EventsAwaiter eventsAwaiter, PublishingRouter publishingRouter)
        {
            this.logger = logger;
            this.eventsAwaiter = eventsAwaiter;
            this.publishingRouter = publishingRouter;
        }

        [HttpPost]
        [Route("UserActivity")]
        public async Task<string> UserActivity(UserActivityReportRequest data)
        {
            var payload = new GenerateUserActivityReportEvent
            {
                Granularity = data.Granularity,
                TimestampFrom = data.TimestampFrom,
                TimestampTo = data.TimestampTo,
                UserId = data.UserId,
            };
            var flowId = HttpContext.Items["flowId"].ToString();
            var response = await eventsAwaiter.AwaitResponse<ReportCreatedEvent>(flowId, () => publishingRouter.Publish(Queues.Transactions, payload, flowId, Queues.APIGateway));
            return response.Report;
        }

        [HttpPost]
        [Route("Overall")]
        public async Task<string> Overall(OverallReportRequest data)
        {
            var payload = new GenerateOverallReportEvent
            {
                Granularity = data.Granularity,
                TimestampFrom = data.TimestampFrom,
                TimestampTo = data.TimestampTo,
                Aggregations = data.Aggregations,
                Subject = data.Subject,
            };
            var flowId = HttpContext.Items["flowId"].ToString();
            var response = await eventsAwaiter.AwaitResponse<ReportCreatedEvent>(flowId, () => publishingRouter.Publish(Queues.Transactions, payload, flowId, Queues.APIGateway));
            return response.Report;
        }
    }
}
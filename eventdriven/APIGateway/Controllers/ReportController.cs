using System.Threading.Tasks;
using APIGateway.Models;
using APIGateway.Reports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedClasses.Events.Reports;
using SharedClasses.Messaging;
using SharedClasses.Messaging.RabbitMq;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> logger;
        private readonly EventsAwaiter eventsAwaiter;
        private readonly PublishingRouter publishingRouter;
        private readonly string queue;

        public ReportController(ILogger<ReportController> logger, EventsAwaiter eventsAwaiter, PublishingRouter publishingRouter, RabbitMqConfig config)
        {
            this.logger = logger;
            this.eventsAwaiter = eventsAwaiter;
            this.publishingRouter = publishingRouter;
            queue = config.Queue;
        }

        [HttpPost]
        [Route("UserActivity")]
        public async Task<string> UserActivity(UserActivityReportRequest data)
        {
            var payload = new AggregateUserActivityReportDataEvent
            {
                Granularity = data.Granularity,
                TimestampFrom = data.TimestampFrom,
                TimestampTo = data.TimestampTo,
                UserId = data.UserId,
            };
            var flowId = HttpContext.Items["flowId"].ToString();
            var response = await eventsAwaiter.AwaitResponse<AggregatedUserActivityReportEvent>(flowId, () => publishingRouter.Publish(Queues.Transactions, payload, flowId, queue));

            return ReportCsvSerializer.SerializerUserActivityReport(data.UserId, data.TimestampFrom, data.TimestampTo, data.Granularity, response);
        }

        [HttpPost]
        [Route("Overall")]
        public async Task<string> Overall(OverallReportRequest data)
        {
            var payload = new AggregateOverallReportDataEvent
            {
                Granularity = data.Granularity,
                TimestampFrom = data.TimestampFrom,
                TimestampTo = data.TimestampTo,
                Aggregations = data.Aggregations,
                Subject = data.Subject,
            };
            var flowId = HttpContext.Items["flowId"].ToString();
            var response = await eventsAwaiter.AwaitResponse<AggregatedOverallReportEvent>(flowId, () => publishingRouter.Publish(Queues.Transactions, payload, flowId, queue));

            return ReportCsvSerializer.SerializerOverallReport(data.Subject, data.TimestampFrom, data.TimestampTo, data.Granularity, response.Portions);
        }
    }
}
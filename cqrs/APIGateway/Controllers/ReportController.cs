using System.Linq;
using System.Threading.Tasks;
using APIGateway.Models;
using APIGateway.Reports;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> logger;
        private readonly Mapper mapper;
        private readonly ReportDataFetcher reportDataFetcher;

        public ReportController(ILogger<ReportController> logger, Mapper mapper, ReportDataFetcher reportDataFetcher)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.reportDataFetcher = reportDataFetcher;
        }

        [HttpPost]
        [Route("UserActivity")]
        public async Task<string> UserActivity(UserActivityReportRequest request)
        {
            var flowId = HttpContext.Items["flowId"].ToString();
            var granularity =  mapper.Map<Granularity>(request.Granularity);
            var portions = await reportDataFetcher.GetUserActivityPortions(flowId, request.UserId, request.TimestampFrom, request.TimestampTo, granularity);
            var csv = ReportCsvSerializer.SerializerUserActivityReport(request.UserId, request.TimestampFrom, request.TimestampTo, request.Granularity, portions);
            return csv;
        }

        [HttpPost]
        [Route("Overall")]
        public async Task<string> Overall(OverallReportRequest request)
        {
            var flowId = HttpContext.Items["flowId"].ToString();
            var cqrsRequest = new AggregateOverallRequest
            {
                FlowId = flowId,
                Granularity = mapper.Map<Granularity>(request.Granularity),
                TimestampFrom = request.TimestampFrom.HasValue ? request.TimestampFrom.Value.Ticks : 0,
                TimestampTo = request.TimestampTo.HasValue ? request.TimestampTo.Value.Ticks : 0,
                Aggregations = { request.Aggregations.Select(a => mapper.Map<Aggregation>(a)) }
            };
            var data = await reportDataFetcher.GetOverallReportPortions(cqrsRequest, request.Subject);
            var csv = ReportCsvSerializer.SerializerOverallReport(request.Subject,
              request.TimestampFrom,
              request.TimestampTo,
              request.Granularity,
              data);
            return csv;
        }
    }
}
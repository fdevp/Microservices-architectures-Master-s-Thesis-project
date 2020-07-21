using System;
using System.Linq;
using System.Threading.Tasks;
using APIGateway.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReportsBranchMicroservice;
using static ReportsBranchMicroservice.ReportsBranch;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ILogger<ReportController> logger;
        private readonly Mapper mapper;
        private readonly ReportsBranchClient reportsBranchClient;

        public ReportController(ILogger<ReportController> logger, Mapper mapper, ReportsBranchClient reportsBranchClient)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.reportsBranchClient = reportsBranchClient;
        }

        [HttpPost]
        [Route("UserActivity")]
        public async Task<string> UserActivity(UserActivityReportRequest data)
        {
            var request = new GenerateUserActivityReportRequest
            {
                Granularity = mapper.Map<Granularity>(data.Granularity),
                TimestampFrom = data.TimestampFrom.HasValue ? data.TimestampFrom.Value.Ticks : 0,
                TimestampTo = data.TimestampTo.HasValue ? data.TimestampTo.Value.Ticks : 0,
                UserId = data.UserId,
            };
            request.FlowId = (long)HttpContext.Items["flowId"];
            var response = await reportsBranchClient.GenerateUserActivityReportAsync(request);
            return response.Report;
        }

        [HttpPost]
        [Route("Overall")]
        public async Task<string> Overall(OverallReportRequest data)
        {
            var request = new GenerateOverallReportRequest
            {
                Granularity = mapper.Map<Granularity>(data.Granularity),
                TimestampFrom = data.TimestampFrom.HasValue ? data.TimestampFrom.Value.Ticks : 0,
                TimestampTo = data.TimestampTo.HasValue ? data.TimestampTo.Value.Ticks : 0,
                Aggregations = { data.Aggregations.Select(a => mapper.Map<Aggregation>(a)) },
                Subject = mapper.Map<ReportSubject>(data.Subject),
            };
            request.FlowId = (long)HttpContext.Items["flowId"];

            var response = await reportsBranchClient.GenerateOverallReportAsync(request);
            return response.Report;
        }
    }
}
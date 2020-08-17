using System;
using System.Linq;
using System.Threading.Tasks;
using APIGateway.Models;
using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReportsBranchMicroservice;
using SharedClasses;
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
                TimestampFrom = data.TimestampFrom.ToNullableTimestamp(),
                TimestampTo = data.TimestampTo.ToNullableTimestamp(),
                UserId = data.UserId,
            };
            request.FlowId = HttpContext.Items["flowId"].ToString();
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
                TimestampFrom = data.TimestampFrom.ToNullableTimestamp(),
                TimestampTo = data.TimestampTo.ToNullableTimestamp(),
                Aggregations = { data.Aggregations.Select(a => mapper.Map<Aggregation>(a)) },
                Subject = mapper.Map<ReportSubject>(data.Subject),
            };
            request.FlowId = HttpContext.Items["flowId"].ToString();

            var response = await reportsBranchClient.GenerateOverallReportAsync(request);
            return response.Report;
        }
    }
}
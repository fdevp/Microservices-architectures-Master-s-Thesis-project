using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace ReportsMicroservice
{
    public class ReportsService : Reports.ReportsBase
    {
        private readonly ILogger<ReportsService> logger;
        private readonly DataFetcher dataFetcher;

        public ReportsService(ILogger<ReportsService> logger, DataFetcher dataFetcher)
        {
            this.logger = logger;
            this.dataFetcher = dataFetcher;
        }

        public override async Task<GenerateReportResponse> GenerateOverallReport(GenerateOverallReportRequest request, ServerCallContext context)
        {
            var dataRequest = new AggregateOverallRequest
            {
                FlowId = request.FlowId,
                Aggregations = { request.Aggregations },
                TimestampFrom = request.TimestampFrom,
                TimestampTo = request.TimestampTo,
                Granularity = request.Granularity,
            };

            var data = await dataFetcher.GetOverallReportPortions(dataRequest, request.Subject);
            var csv = ReportGenerator.CreateOverallCsvReport(request.Subject,
              GetDateTime(request.TimestampFrom),
              GetDateTime(request.TimestampTo),
              request.Granularity,
              data);

            return new GenerateReportResponse { FlowId = request.FlowId, Report = csv };
        }

        public override async Task<GenerateReportResponse> GenerateUserActivityReport(GenerateUserActivityReportRequest request, ServerCallContext context)
        {
            var data = new UserActivityRaportData
            {
                UserId = request.UserId,
                From = GetDateTime(request.TimestampFrom),
                To = GetDateTime(request.TimestampTo),
                Granularity = request.Granularity,
            };

            data.Portions = await dataFetcher.GetUserActivityPortions(request.FlowId, request.UserId, request.TimestampFrom, request.TimestampTo, request.Granularity);

            var csv = ReportGenerator.CreateUserActivityCsvReport(data);
            return new GenerateReportResponse { FlowId = request.FlowId, Report = csv };
        }

        private DateTime? GetDateTime(long ticks) => ticks > 0 ? new DateTime(ticks) : null as DateTime?;
    }


}

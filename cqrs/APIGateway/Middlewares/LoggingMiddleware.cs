using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace APIGateway.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var flowId = FlowIdProvider.Create();
            httpContext.Items.Add("flowId", flowId);

            logger.LogInformation($"Service='APIGateway' FlowId='{flowId}' Method='{httpContext.Request.Path}' Type='Start'");
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await next(httpContext);
            }
            finally
            {
                logger.LogInformation($"Service='APIGateway' FlowId='{flowId}' Method='{httpContext.Request.Path}' Type='End' Processing='{stopwatch.ElapsedMilliseconds}'");
            }
        }
    }
}
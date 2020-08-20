using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace SharedClasses
{
    public class LoggingInterceptor : Interceptor
    {
        private readonly ILogger<LoggingInterceptor> logger;
        private readonly string serviceName;

        public LoggingInterceptor(ILogger<LoggingInterceptor> logger, string serviceName)
        {
            this.logger = logger;
            this.serviceName = serviceName;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, Grpc.Core.ServerCallContext context, Grpc.Core.UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var method = context.Method;
            var flowId = context.RequestHeaders.FirstOrDefault(h => h.Key == "flowid")?.Value;

            this.logger.LogInformation($"Service='{serviceName}' FlowId='{flowId}' Method='{method}' Type='Start'");
            var stopwatch = Stopwatch.StartNew();
            try
            {
                return await base.UnaryServerHandler(request, context, continuation);
            }
            finally
            {
                logger.LogInformation($"Service='{serviceName}' FlowId='{flowId}' Method='{method}' Type='End' Processing='{stopwatch.ElapsedMilliseconds}'");
            }
        }
    }
}
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace SharedClasses
{
    public class LoggingInterceptor : Interceptor
    {
        private const string FailureMessage = "Failure test";
        private readonly ILogger<LoggingInterceptor> logger;
        private readonly string serviceName;
        private readonly FailureSettings failureSettings;

        public LoggingInterceptor(ILogger<LoggingInterceptor> logger, string serviceName, FailureSettings failureSettings)
        {
            this.logger = logger;
            this.serviceName = serviceName;
            this.failureSettings = failureSettings;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, Grpc.Core.ServerCallContext context, Grpc.Core.UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var method = context.Method;
            var flowId = GetFlowId(request);
            
            var isGuid = Guid.TryParse(flowId, out var guid);
            if (isGuid && guid.GetHashCode() % failureSettings.ComponentsCount == failureSettings.ComponentNumber)
            {
                throw new RpcException(new Status(StatusCode.Unavailable, FailureMessage));
            }

            this.logger.LogInformation($"Service='{serviceName}' FlowId='{flowId}' Method='{method}' Type='Start'");
            var stopwatch = Stopwatch.StartNew();
            try
            {
                return await base.UnaryServerHandler(request, context, continuation);
            }
            catch (RpcException e)
            {
                if (e.StatusCode == StatusCode.Unavailable)
                    logger.LogInformation($"Service='{serviceName}' FlowId='{flowId}' Method='{method}' Type='Error'");
                throw e;
            }
            finally
            {
                logger.LogInformation($"Service='{serviceName}' FlowId='{flowId}' Method='{method}' Type='End' Processing='{stopwatch.ElapsedMilliseconds}'");
            }
        }
    }
}
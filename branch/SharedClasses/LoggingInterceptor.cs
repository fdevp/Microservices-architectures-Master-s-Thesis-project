using System;
using System.Diagnostics;
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
        private readonly int componentNumber;
        private readonly int componentsCount;

        public LoggingInterceptor(ILogger<LoggingInterceptor> logger, string serviceName, int componentNumber, int componentsCount)
        {
            this.logger = logger;
            this.serviceName = serviceName;
            this.componentNumber = componentNumber;
            this.componentsCount = componentsCount;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, Grpc.Core.ServerCallContext context, Grpc.Core.UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var method = context.Method;
            var flowId = GetFlowId(request);

            if (new Guid(flowId).GetHashCode() % componentsCount == componentNumber)
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
                    logger.LogInformation($"Service='{serviceName}' Type='Error' FlowId='{flowId}' Method='{method}' Type='End' Processing='{stopwatch.ElapsedMilliseconds}'");
                throw e;
            }
            finally
            {
                logger.LogInformation($"Service='{serviceName}' Type='Success' FlowId='{flowId}' Method='{method}' Type='End' Processing='{stopwatch.ElapsedMilliseconds}'");
            }
        }

        private string GetFlowId<TRequest>(TRequest request)
        {
            Type t = request.GetType();
            PropertyInfo prop = t.GetProperty("FlowId");
            return prop?.GetValue(request) as string;
        }
    }
}
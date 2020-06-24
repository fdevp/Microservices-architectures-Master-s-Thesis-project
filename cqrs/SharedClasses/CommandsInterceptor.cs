using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using SharedClasses.Commands;

namespace SharedClasses
{
    public class CommandsInterceptor : Interceptor
    {
        private readonly CommandsRepository commandsRepository;

        public CommandsInterceptor(CommandsRepository commandsRepository)
        {
            this.commandsRepository = commandsRepository;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, Grpc.Core.ServerCallContext context, Grpc.Core.UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var method = context.Method;
            var flowId = GetFlowId(request);

            var result = await base.UnaryServerHandler(request, context, continuation);
            if(flowId.HasValue)
                commandsRepository.Add(request, flowId.Value);

            return result;
        }

        private long? GetFlowId<TRequest>(TRequest request)
        {
            Type t = request.GetType();
            PropertyInfo prop = t.GetProperty("FlowId");
            return prop?.GetValue(request) as long?;
        }
    }
}
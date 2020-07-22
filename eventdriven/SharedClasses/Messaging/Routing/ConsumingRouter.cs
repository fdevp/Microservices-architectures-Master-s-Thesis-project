using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Jil;
using Microsoft.Extensions.Logging;

namespace SharedClasses.Messaging
{
    public class ConsumingRouter<T>
    {
        private IReadOnlyDictionary<string, MethodInfo> routing;
        private readonly string serviceName;
        private readonly ILogger logger;
        private readonly T service;

        private ConsumingRouter(T service, IReadOnlyDictionary<string, MethodInfo> routing, string serviceName, ILogger logger)
        {
            this.service = service;
            this.routing = routing;
            this.serviceName = serviceName;
            this.logger = logger;
        }

        public static ConsumingRouter<T> Create(T service, string serviceName, ILogger logger)
        {
            var serviceType = service.GetType();
            var methods = serviceType.GetMethods();
            var eventHandlers = GetEventHandlersMethods(methods).ToDictionary(k => k.typeName, v => v.method);
            return new ConsumingRouter<T>(service, eventHandlers, serviceName, logger);
        }

        public void LinkConsumer(IConsumer consumer)
        {
            consumer.Received += RouteEvent;
        }

        private async void RouteEvent(object sender, MqMessage message)
        {
            if (!routing.ContainsKey(message.Type))
                return;

            var method = routing[message.Type];
            var data = Parse(message.Type, message.Data);
            var context = new MessageContext { FlowId = message.FlowId, Type = message.Type, ReplyTo = message.ReplyTo };

            logger.LogInformation($"Service='{serviceName}' FlowId='{message.FlowId}' Method='{message.Type}' Type='Start'");
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await ((Task)method.Invoke(service, new[] { context, data }));
            }
            finally
            {
                logger.LogInformation($"Service='{serviceName}' FlowId='{message.FlowId}' Method='{message.Type}' Type='End' Processing='{stopwatch.ElapsedMilliseconds}'");
            }
        }

        private object Parse(string typeName, string message)
        {
            var type = Type.GetType(typeName);
            return JSON.Deserialize(message, type, Options.ISO8601Utc);
        }

        private static IEnumerable<(string typeName, MethodInfo method)> GetEventHandlersMethods(MethodInfo[] methods)
        {
            foreach (var method in methods)
            {
                var eventHandlerAttribute = method.GetCustomAttribute(typeof(EventHandlingMethod)) as EventHandlingMethod;
                if (eventHandlerAttribute != null)
                    yield return (eventHandlerAttribute.Type, method);
            }
        }
    }
}
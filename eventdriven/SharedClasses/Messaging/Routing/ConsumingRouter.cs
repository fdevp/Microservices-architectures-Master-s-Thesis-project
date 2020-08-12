using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Jil;
using Microsoft.Extensions.Logging;
using SharedClasses.Events;

namespace SharedClasses.Messaging
{
    public class ConsumingRouter<T>
    {
        private IReadOnlyDictionary<string, MethodInfo> routing;
        private readonly PublishingRouter publishingRouter;
        private readonly string serviceName;
        private readonly ILogger logger;
        private readonly FailureSettings failureSettings;
        private readonly T service;

        private ConsumingRouter(T service, IReadOnlyDictionary<string, MethodInfo> routing, PublishingRouter publishingRouter, string serviceName, ILogger logger, FailureSettings failureSettings)
        {
            this.service = service;
            this.routing = routing;
            this.publishingRouter = publishingRouter;
            this.serviceName = serviceName;
            this.logger = logger;
            this.failureSettings = failureSettings;
        }

        public static ConsumingRouter<T> Create(T service, PublishingRouter publishingRouter, string serviceName, ILogger logger, FailureSettings failureConfig)
        {
            var serviceType = service.GetType();
            var methods = serviceType.GetMethods();
            var eventHandlers = GetEventHandlersMethods(methods).ToDictionary(k => k.typeName, v => v.method);
            return new ConsumingRouter<T>(service, eventHandlers, publishingRouter, serviceName, logger, failureConfig);
        }

        public void LinkConsumer(IConsumer consumer)
        {
            consumer.Received += RouteEvent;
        }

        private async void RouteEvent(object sender, MqMessage message)
        {
            if (!routing.ContainsKey(message.Type))
                return;


            if (message.ReplyTo != null)
            {
                var flowId = message.FlowId.IndexOf('_') >= 0 ? message.FlowId.Split("_").First() : message.FlowId;
                var isGuid = Guid.TryParse(message.FlowId, out var guid);
                if (isGuid && guid.GetHashCode() % failureSettings.ComponentsCount == failureSettings.ComponentNumber)
                    return;
            }

            var method = routing[message.Type];
            var data = Parse(message.Type, message.Data);
            var context = new MessageContext { FlowId = message.FlowId, Type = message.Type, ReplyTo = message.ReplyTo };

            logger.LogInformation($"Service='{serviceName}' FlowId='{message.FlowId}' Method='{message.Type}' Type='Start'");
            var stopwatch = Stopwatch.StartNew();
            try
            {
                await ((Task)method.Invoke(service, new[] { context, data }));
            }
            catch (Exception e)
            {
                if (message.ReplyTo != null)
                    this.publishingRouter.Publish(message.ReplyTo, new ErrorEvent { Error = e.GetType().Name, Message = e.Message }, message.FlowId);
                logger.LogInformation($"Service='{serviceName}' FlowId='{message.FlowId}' Method='{message.Type}' Type='Error'");
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jil;

namespace SharedClasses.Messaging
{
    public class ConsumingRouter<T>
    {
        private IReadOnlyDictionary<string, MethodInfo> routing;
        private readonly T service;

        private ConsumingRouter(T service, IReadOnlyDictionary<string, MethodInfo> routing)
        {
            this.service = service;
            this.routing = routing;
        }

        public static ConsumingRouter<T> Create(T service)
        {
            var serviceType = service.GetType();
            var methods = serviceType.GetMethods();
            var eventHandlers = GetEventHandlersMethods(methods).ToDictionary(k => k.typeName, v => v.method);
            return new ConsumingRouter<T>(service, eventHandlers);
        }

        public void LinkConsumer(IConsumer consumer)
        {
            consumer.Received += RouteEvent;
        }

        private void RouteEvent(object sender, MqMessage message)
        {
            if (!routing.ContainsKey(message.Type))
                throw new InvalidOperationException("Unknown event.");

            var method = routing[message.Type];
            var data = Parse(message.Type, message.Data);
            var context = new MessageContext { FlowId = message.FlowId, Type = message.Type, ReplyTo = message.ReplyTo };
            method.Invoke(service, new[] { context, data });
        }

        private object Parse(string typeName, string message)
        {
            var type = Type.GetType(typeName);
            return JSON.Deserialize(message, type);
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
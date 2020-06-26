using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        public ConsumingRouter<T> Create(T service)
        {
            var serviceType = service.GetType();
            var methods = serviceType.GetMethods();
            var eventHandlers = GetEventHandlersMethods(methods).ToDictionary(k => k.typeName, v => v.method);
            return new ConsumingRouter<T>(service, eventHandlers);
        }

        public void RouteEvent(object sender, MqMessage message)
        {
            if (!routing.ContainsKey(message.Type))
                throw new InvalidOperationException("Unknown event routing.");

            var method = routing[message.Type];
            var data = MessageParser.Parse(message.Type, message.Data);
            method.Invoke(service, new[] { message.FlowId, data });
        }

        private IEnumerable<(string typeName, MethodInfo method)> GetEventHandlersMethods(MethodInfo[] methods)
        {
            foreach (var method in methods)
            {
                var eventHandlerAttribute = (EventHandler)method.GetCustomAttribute(typeof(EventHandler));
                if (eventHandlerAttribute != null)
                    yield return (eventHandlerAttribute.Type, method);
            }
        }
    }
}
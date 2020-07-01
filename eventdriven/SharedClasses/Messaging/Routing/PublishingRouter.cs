using System;
using System.Collections.Generic;

namespace SharedClasses.Messaging
{
    public class PublishingRouter
    {
        private readonly IReadOnlyDictionary<string, IPublisher> publishers;

        public PublishingRouter(IReadOnlyDictionary<string, IPublisher> publishers)
        {
            this.publishers = publishers;
        }

        public void Publish(string queueName, object payload, string flowId, string replyTo = null)
        {
            if (!publishers.ContainsKey(queueName))
                throw new InvalidOperationException($"There is no defined publisher for queue '{queueName}'.");

            var type = payload.GetType().ToString();
            publishers[queueName].Publish(type, payload, flowId, replyTo);
        }
    }
}
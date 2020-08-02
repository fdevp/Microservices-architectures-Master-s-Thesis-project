using System;
using System.IO;
using System.Text;
using Jil;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace SharedClasses.Messaging
{
    public class RabbitMqPublisher : IDisposable
    {
        private readonly IModel channel;
        private readonly string queueName;
        private readonly string serviceName;
        private readonly ILogger logger;

        public RabbitMqPublisher(IModel channel, string queueName, string serviceName, ILogger logger)
        {
            this.channel = channel;
            this.queueName = queueName;
            this.serviceName = serviceName;
            this.logger = logger;
        }

        public void Publish(string flowId, object content)
        {
            logger.LogInformation($"Service='{serviceName}' FlowId='{flowId}' Method='Projection' Type='Start'");

            var properties = channel.CreateBasicProperties();
            properties.CorrelationId = flowId;
            using (var sw = new StringWriter())
            {
                JSON.Serialize(content, sw);
                var body = Encoding.UTF8.GetBytes(sw.ToString());

                lock (channel)
                {
                    channel.BasicPublish(exchange: "",
                                    routingKey: queueName,
                                    basicProperties: properties,
                                    body: body);
                }
            }
        }

        public void Dispose()
        {
            channel.Dispose();
        }
    }


}
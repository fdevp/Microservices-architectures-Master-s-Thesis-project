using System;
using System.IO;
using System.Text;
using Jil;
using RabbitMQ.Client;

namespace SharedClasses.Messaging.RabbitMq
{
    public class RabbitMqPublisher : IPublisher, IDisposable
    {
        private readonly IModel channel;
        private readonly string queueName;

        public RabbitMqPublisher(IModel channel, string queueName)
        {
            this.channel = channel;
            this.queueName = queueName;
        }

        public void Publish(object content, string flowId)
        {
            using (var sw = new StringWriter())
            {
                JSON.Serialize(content, sw);
                var body = Encoding.UTF8.GetBytes(sw.ToString());
                var properties = channel.CreateBasicProperties();
                properties.CorrelationId = flowId;

                channel.BasicPublish(exchange: "",
                routingKey: queueName,
                basicProperties: properties,
                body: body);
            }
        }

        public void Dispose()
        {
            channel.Dispose();
        }
    }

}
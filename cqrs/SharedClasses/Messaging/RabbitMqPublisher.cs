using System;
using System.IO;
using System.Text;
using Jil;
using RabbitMQ.Client;

namespace SharedClasses.Messaging
{
    public class RabbitMqPublisher : IDisposable
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string queueName;
        private readonly IBasicProperties properties;

        public RabbitMqPublisher(IConnection connection, IModel channel, string queueName, IBasicProperties properties)
        {
            this.connection = connection;
            this.channel = channel;
            this.queueName = queueName;
            this.properties = properties;
        }

        public void Publish(string msgType, object content)
        {
            using (var sw = new StringWriter())
            {
                JSON.Serialize(content, sw);
                var body = Encoding.UTF8.GetBytes(sw.ToString());

                channel.BasicPublish(exchange: "",
                routingKey: queueName,
                basicProperties: properties,
                body: body);
            }
        }

        public void Dispose()
        {
            channel.Dispose();
            connection.Dispose();
        }
    }


}
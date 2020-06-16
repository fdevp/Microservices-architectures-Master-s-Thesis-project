using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SharedClasses.Messaging
{
    public class RabbitMqConsumer : IDisposable
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string queueName;
        private readonly EventingBasicConsumer consumer;

        public RabbitMqConsumer(IConnection connection, IModel channel, string queueName, EventingBasicConsumer consumer)
        {
            this.connection = connection;
            this.channel = channel;
            this.queueName = queueName;
            this.consumer = consumer;
        }

        

        public void Dispose()
        {
            channel.Dispose();
            connection.Dispose();   
        }
    }
}
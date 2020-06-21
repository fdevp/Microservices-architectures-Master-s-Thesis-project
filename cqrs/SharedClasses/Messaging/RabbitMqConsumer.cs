using System;
using System.Text;
using Jil;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SharedClasses.Messaging
{
    public class RabbitMqConsumer<TUpsert, TRemove> : IDisposable
    {
        public event EventHandler<DataProjection<TUpsert, TRemove>> Received;
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string queueName;
        private EventingBasicConsumer consumer;

        public RabbitMqConsumer(IConnection connection, IModel channel, string queueName, EventingBasicConsumer consumer)
        {
            this.connection = connection;
            this.channel = channel;
            this.queueName = queueName;
            this.consumer = consumer;
        }

        private void HandleMessage(object sender, BasicDeliverEventArgs ea)
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            var projection = JSON.Deserialize<DataProjection<TUpsert, TRemove>>(message);
            this.Received.Invoke(sender, projection);
        }

        public void Dispose()
        {
            channel.Dispose();
            connection.Dispose();
        }
    }
}
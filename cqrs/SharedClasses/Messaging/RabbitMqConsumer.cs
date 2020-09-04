using System;
using System.Text;
using Jil;
using Microsoft.Extensions.Logging;
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
        private readonly string serviceName;
        private readonly ILogger logger;
        private EventingBasicConsumer consumer;

        public RabbitMqConsumer(IModel channel, string queueName, string serviceName, ILogger logger, EventingBasicConsumer consumer)
        {
            this.channel = channel;
            this.queueName = queueName;
            this.serviceName = serviceName;
            this.logger = logger;
            this.consumer = consumer;
            this.consumer.Received += HandleMessage;
        }

        private void HandleMessage(object sender, BasicDeliverEventArgs ea)
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            var projection = JSON.Deserialize<DataProjection<TUpsert, TRemove>>(message);
            this.Received.Invoke(sender, projection);
            logger.LogInformation($"Service='{serviceName}' FlowId='{ea.BasicProperties.CorrelationId}' Method='Projection' Type='End'");
        }

        public void Dispose()
        {
            channel.Dispose();
        }
    }
}
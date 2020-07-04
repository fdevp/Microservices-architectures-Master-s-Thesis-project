using System;
using System.Text;
using Jil;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SharedClasses.Messaging.RabbitMq
{
    public class RabbitMqConsumer : IConsumer, IDisposable
    {
        public event EventHandler<MqMessage> Received;
        private readonly IModel channel;
        private readonly string queueName;

        private RabbitMqConsumer(IModel channel, string queueName)
        {
            this.channel = channel;
            this.queueName = queueName;
        }

        public static RabbitMqConsumer Create(IModel channel, string queueName)
        {
            var rabbitConsumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue: queueName, autoAck: true, consumer: rabbitConsumer);

            var consumer = new RabbitMqConsumer(channel, queueName);
            consumer.LinkRabbitConsumer(rabbitConsumer);

            return consumer;
        }

        private void LinkRabbitConsumer(EventingBasicConsumer eventConsumer)
        {
            eventConsumer.Received += HandleMessage;
        }

        private void HandleMessage(object sender, BasicDeliverEventArgs ea)
        {
            var data = Encoding.UTF8.GetString(ea.Body.ToArray());
            var message = new MqMessage(data, ea.BasicProperties.CorrelationId, ea.BasicProperties.Type, ea.BasicProperties.ReplyTo);
            this.Received.Invoke(this, message);
        }

        public void Dispose()
        {
            channel.Dispose();
        }
    }
}
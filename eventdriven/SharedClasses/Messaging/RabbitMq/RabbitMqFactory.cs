using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SharedClasses.Messaging.RabbitMq
{
    public class RabbitMqFactory
    {
        public IConsumer CreateConsumer(RabbitMqConfig config)
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = config.HostName,
                UserName = config.UserName,
                Password = config.Password,
            };

            var connection = connectionFactory.CreateConnection();

            var channel = connection.CreateModel();
            channel.QueueDeclare(config.QueueName, true, false, false);

            return RabbitMqConsumer.Create(channel, config.QueueName);
        }

        public IPublisher CreatePublisher(RabbitMqConfig config)
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = config.HostName,
                UserName = config.UserName,
                Password = config.Password,
            };

            var connection = connectionFactory.CreateConnection();
            var channel = connection.CreateModel();
            channel.QueueDeclare(config.QueueName, true, false, false);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            return new RabbitMqPublisher(channel, config.QueueName);
        }
    }
}
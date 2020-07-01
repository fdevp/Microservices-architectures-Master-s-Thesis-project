using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SharedClasses.Messaging.RabbitMq
{
    public class RabbitMqFactory
    {
        private IConnection connection;

        private RabbitMqFactory(IConnection connection)
        {
            this.connection = connection;
        }

        public static RabbitMqFactory Create(RabbitMqConfig config)
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = config.HostName,
                UserName = config.UserName,
                Password = config.Password,
            };

            var connection = connectionFactory.CreateConnection();
            return new RabbitMqFactory(connection);
        }

        public IConsumer CreateConsumer(string queueName)
        {
            var channel = connection.CreateModel();
            channel.QueueDeclare(queueName, true, false, false);

            return RabbitMqConsumer.Create(channel, queueName);
        }

        public IPublisher CreatePublisher(string queueName)
        {
            var channel = connection.CreateModel();
            channel.QueueDeclare(queueName, true, false, false);

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            return new RabbitMqPublisher(channel, queueName);
        }
    }
}
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SharedClasses.Messaging
{
    public class RabbitMqChannelFactory
    {
        public RabbitMqConsumer<TUpsert, TRemove> CreateReadChannel<TUpsert, TRemove>(RabbitMqConfig config, string serviceName, ILogger logger)
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

            var consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(
                            queue: config.QueueName,
                            autoAck: true,
                            consumer: consumer);

            return new RabbitMqConsumer<TUpsert, TRemove>(channel, config.QueueName, serviceName, logger, consumer);
        }

        public RabbitMqPublisher CreateWriteChannel(RabbitMqConfig config, string serviceName, ILogger logger)
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

            return new RabbitMqPublisher(channel, config.QueueName, serviceName, logger);
        }
    }
}
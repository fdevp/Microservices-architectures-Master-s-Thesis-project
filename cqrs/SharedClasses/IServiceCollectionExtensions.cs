using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedClasses.Messaging;

namespace SharedClasses
{
    public static class IServiceCollectionExtensions
    {
        public static RabbitMqPublisher AddRabbitMqPublisher(this IServiceCollection collection, IConfiguration configuration, string serviceName)
        {
            var config = new RabbitMqConfig();
            configuration.GetSection("RabbitMq").Bind(config);
            var logger = collection.BuildServiceProvider().GetService<ILogger<RabbitMqPublisher>>();
            var publisher = new RabbitMqChannelFactory().CreateWriteChannel(config, serviceName, logger);
            collection.AddSingleton(publisher);
            return publisher;
        }
    }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedClasses.Messaging;

namespace SharedClasses
{
    public static class IServiceCollectionExtensions
    {
        public static RabbitMqPublisher AddRabbitMqPublisher(this IServiceCollection collection, IConfiguration configuration)
        {
            var config = new RabbitMqConfig();
            configuration.GetSection("RabbitMq").Bind(config);
            var publisher = new RabbitMqChannelFactory().CreateWriteChannel(config);
            collection.AddSingleton(publisher);
            return publisher;
        }
    }
}
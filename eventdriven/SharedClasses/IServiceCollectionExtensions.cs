using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedClasses.Messaging;
using SharedClasses.Messaging.RabbitMq;

namespace SharedClasses
{
    public static class IServiceCollectionExtensions
    {
        public static IPublisher AddRabbitMqPublisher(this IServiceCollection collection, IConfiguration configuration)
        {
            var config = new RabbitMqConfig();
            configuration.GetSection("RabbitMq").Bind(config);
            var publisher = new RabbitMqFactory().CreatePublisher(config);
            collection.AddSingleton(publisher);
            return publisher;
        }
    }
}
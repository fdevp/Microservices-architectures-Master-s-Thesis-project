using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardsMicroservice.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SharedClasses.Messaging;
using SharedClasses.Messaging.RabbitMq;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace CardsMicroservice
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(c => c.AddSerilog().AddFile("log.txt"));
            var loggerServicesProvier = services.BuildServiceProvider();
            services.AddSingleton<CardsRepository>();
            services.AddSingleton<CardsService>();
            ConfigureRabbitMq(services, loggerServicesProvier);
        }

        private void ConfigureRabbitMq(IServiceCollection services, ServiceProvider loggerServicesProvier)
        {
            var config = new RabbitMqConfig();
            Configuration.GetSection("RabbitMq").Bind(config);
            var factory = RabbitMqFactory.Create(config);

            var cardsConsumer = factory.CreateConsumer(Queues.Cards);
            var awaiter = new EventsAwaiter("Cards", loggerServicesProvier.GetService<ILogger<EventsAwaiter>>());
            awaiter.BindConsumer(cardsConsumer);
            services.AddSingleton(awaiter);

            var publishers = new Dictionary<string, IPublisher>();
            publishers.Add(Queues.APIGateway, factory.CreatePublisher(Queues.APIGateway));
            publishers.Add(Queues.Accounts, factory.CreatePublisher(Queues.Accounts));
            publishers.Add(Queues.Transactions, factory.CreatePublisher(Queues.Transactions));
            publishers.Add("apigateway1", factory.CreatePublisher("apigateway1"));
            publishers.Add("apigateway2", factory.CreatePublisher("apigateway2"));
            publishers.Add("apigateway3", factory.CreatePublisher("apigateway3"));
            var publishingRouter = new PublishingRouter(publishers);
            services.AddSingleton(publishingRouter);

            var servicesProvider = services.BuildServiceProvider();
            var cardsService = servicesProvider.GetService<CardsService>();

            var consumingRouter = ConsumingRouter<CardsService>.Create(cardsService, publishingRouter, "Cards", loggerServicesProvier.GetService<ILogger<IConsumer>>());
            consumingRouter.LinkConsumer(cardsConsumer);
            services.AddSingleton(consumingRouter);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

        }
    }
}

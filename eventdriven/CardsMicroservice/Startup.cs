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
            services.AddSingleton<CardsRepository>();
            services.AddSingleton<CardsService>();
            ConfigureRabbitMq(services);
        }

        private void ConfigureRabbitMq(IServiceCollection services)
        {
            var config = new RabbitMqConfig();
            Configuration.GetSection("RabbitMq").Bind(config);
            var factory = RabbitMqFactory.Create(config);

            var cardsConsumer = factory.CreateConsumer(Queues.Cards);
            var awaiter = new EventsAwaiter("Cards");
            awaiter.BindConsumer(cardsConsumer);
            services.AddSingleton(awaiter);

            var publishers = new Dictionary<string, IPublisher>();
            publishers.Add(Queues.APIGateway, factory.CreatePublisher(Queues.APIGateway));
            publishers.Add(Queues.Accounts, factory.CreatePublisher(Queues.Accounts));
            publishers.Add(Queues.Transactions, factory.CreatePublisher(Queues.Transactions));
            services.AddSingleton(new PublishingRouter(publishers));

            var servicesProvider = services.BuildServiceProvider();
            var logger = servicesProvider.GetService<ILogger<IConsumer>>();
            var cardsService = servicesProvider.GetService<CardsService>();

            var consumingRouter = ConsumingRouter<CardsService>.Create(cardsService, "Cards", logger);
            consumingRouter.LinkConsumer(cardsConsumer);
            services.AddSingleton(consumingRouter);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            
        }
    }
}

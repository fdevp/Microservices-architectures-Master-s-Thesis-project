using System.Collections.Generic;
using AccountsMicroservice.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Serilog;
using SharedClasses;
using SharedClasses.Messaging;
using SharedClasses.Messaging.RabbitMq;

namespace AccountsMicroservice
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(c => c.AddSerilog().AddFile("log.txt"));
            services.AddSingleton<AccountsRepository>();
            services.AddSingleton<AccountsService>();
            ConfigureRabbitMq(services);
        }

        private void ConfigureRabbitMq(IServiceCollection services)
        {
            var failureSettings = new FailureSettings();
            Configuration.GetSection("FailureSettings").Bind(failureSettings);

            var config = new RabbitMqConfig();
            Configuration.GetSection("RabbitMq").Bind(config);
            var factory = RabbitMqFactory.Create(config);

            var publishers = new Dictionary<string, IPublisher>();
            publishers.Add(Queues.APIGateway, factory.CreatePublisher(Queues.APIGateway));
            publishers.Add(Queues.Transactions, factory.CreatePublisher(Queues.Transactions));
            publishers.Add(Queues.Cards, factory.CreatePublisher(Queues.Cards));
            var publishingRouter = new PublishingRouter(publishers);
            services.AddSingleton(publishingRouter);

            var servicesProvider = services.BuildServiceProvider();
            var logger = servicesProvider.GetService<ILogger<IConsumer>>();
            var accountsService = servicesProvider.GetService<AccountsService>();

            var consumingRouter = ConsumingRouter<AccountsService>.Create(accountsService, publishingRouter, "Accounts", logger, failureSettings);
            var consumer = factory.CreateConsumer(Queues.Accounts);
            consumingRouter.LinkConsumer(consumer);
            services.AddSingleton(consumingRouter);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

        }
    }
}

using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using SharedClasses.Messaging;
using SharedClasses.Messaging.RabbitMq;
using TransactionsMicroservice.Repository;

namespace TransactionsMicroservice
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
            services.AddSingleton<TransactionsRepository>();
            services.AddSingleton<TransactionsService>();
            ConfigureRabbitMq(services);
        }

        private void ConfigureRabbitMq(IServiceCollection services)
        {
            var config = new RabbitMqConfig();
            Configuration.GetSection("RabbitMq").Bind(config);
            var factory = RabbitMqFactory.Create(config);

            var publishers = new Dictionary<string, IPublisher>();
            publishers.Add(Queues.APIGateway, factory.CreatePublisher(Queues.APIGateway));
            publishers.Add(Queues.Accounts, factory.CreatePublisher(Queues.Accounts));
            services.AddSingleton(new PublishingRouter(publishers));

            var servicesProvider = services.BuildServiceProvider();
            var logger = servicesProvider.GetService<ILogger<IConsumer>>();
            var transactionsService = servicesProvider.GetService<TransactionsService>();
            var consumingRouter = ConsumingRouter<TransactionsService>.Create(transactionsService, "Transactions", logger);

            var consumer = factory.CreateConsumer(Queues.Transactions);
            consumingRouter.LinkConsumer(consumer);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

        }
    }
}

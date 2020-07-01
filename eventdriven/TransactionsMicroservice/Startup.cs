using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            publishers.Add(Queues.Accounts, factory.CreatePublisher(Queues.APIGateway));
            publishers.Add(Queues.Transactions, factory.CreatePublisher(Queues.Accounts));
            services.AddSingleton(new PublishingRouter(publishers));

            var transactionsService = services.BuildServiceProvider().GetService<TransactionsService>();
            var consumingRouter = ConsumingRouter<TransactionsService>.Create(transactionsService);

            var consumer = factory.CreateConsumer(Queues.Transactions);
            consumingRouter.LinkConsumer(consumer);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddFile("log.txt");
        }
    }
}

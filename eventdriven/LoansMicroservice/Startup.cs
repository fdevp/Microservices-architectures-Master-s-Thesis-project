using System.Collections.Generic;
using LoansMicroservice.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedClasses.Messaging;
using SharedClasses.Messaging.RabbitMq;

namespace LoansMicroservice
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
            services.AddSingleton<LoansRepository>();
            services.AddSingleton<LoansService>();
            ConfigureRabbitMq(services);
        }

        private void ConfigureRabbitMq(IServiceCollection services)
        {
            var config = new RabbitMqConfig();
            Configuration.GetSection("RabbitMq").Bind(config);
            var factory = RabbitMqFactory.Create(config);

            var publishers = new Dictionary<string, IPublisher>();
            publishers.Add(Queues.APIGateway, factory.CreatePublisher(Queues.APIGateway));
            publishers.Add(Queues.Payments, factory.CreatePublisher(Queues.Payments));
            publishers.Add(Queues.Transactions, factory.CreatePublisher(Queues.Transactions));
            services.AddSingleton(new PublishingRouter(publishers));

            var loansService = services.BuildServiceProvider().GetService<LoansService>();

            var consumingRouter = ConsumingRouter<LoansService>.Create(loansService);
            var consumer = factory.CreateConsumer(Queues.Loans);
            consumingRouter.LinkConsumer(consumer);
            services.AddSingleton(consumingRouter);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddFile("log.txt");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

namespace TransactionsLoadBalancer
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
            publishers.Add(Queues.Cards, factory.CreatePublisher(Queues.Cards));
            publishers.Add(Queues.Loans, factory.CreatePublisher(Queues.Loans));
            publishers.Add(Queues.Payments, factory.CreatePublisher(Queues.Payments));
            services.AddSingleton(new PublishingRouter(publishers));

            var balancingQueues = Configuration.GetSection("Queues").GetChildren().Select(v => v.Value).Distinct();
            services.AddSingleton(new BalancingQueues(balancingQueues));
            foreach (var queue in balancingQueues)
                publishers.Add(queue, factory.CreatePublisher(queue));

            var consumer = factory.CreateConsumer(Queues.Transactions);
            var eventsAwaiter = new EventsAwaiter("TransactionsLoadBalancer");
            eventsAwaiter.BindConsumer(consumer);
            services.AddSingleton(eventsAwaiter);

            var servicesProvider = services.BuildServiceProvider();
            var logger = servicesProvider.GetService<ILogger<IConsumer>>();
            var transactionsService = servicesProvider.GetService<TransactionsBalancingService>();
            var consumingRouter = ConsumingRouter<TransactionsBalancingService>.Create(transactionsService, "TransactionsLoadBalancer", logger);
            consumingRouter.LinkConsumer(consumer);
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

        }
    }
}

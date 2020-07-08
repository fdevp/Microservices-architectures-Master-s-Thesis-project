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
using SharedClasses.Messaging;
using SharedClasses.Messaging.RabbitMq;
using UsersMicroservice.Repository;

namespace UsersMicroservice
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<UsersRepository>();
            services.AddSingleton<UsersService>();
            ConfigureRabbitMq(services);
        }

        private void ConfigureRabbitMq(IServiceCollection services)
        {
            var config = new RabbitMqConfig();
            Configuration.GetSection("RabbitMq").Bind(config);
            var factory = RabbitMqFactory.Create(config);

            var publishers = new Dictionary<string, IPublisher>();
            publishers.Add(Queues.APIGateway, factory.CreatePublisher(Queues.APIGateway));
            services.AddSingleton(new PublishingRouter(publishers));

            var usersService = services.BuildServiceProvider().GetService<UsersService>();

            var consumingRouter = ConsumingRouter<UsersService>.Create(usersService, "Users");
            var consumer = factory.CreateConsumer(Queues.Users);
            consumingRouter.LinkConsumer(consumer);
            services.AddSingleton(consumingRouter);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddFile("log.txt");
        }
    }
}

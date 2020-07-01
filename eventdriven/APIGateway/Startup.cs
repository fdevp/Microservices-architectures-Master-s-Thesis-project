using System;
using System.Collections.Generic;
using APIGateway.Middlewares;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SharedClasses.Messaging;
using SharedClasses.Messaging.RabbitMq;

namespace APIGateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();
            services.AddSingleton(CreateMapper());
            ConfigureRabbitMq(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseMiddleware<LoggingMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            loggerFactory.AddFile("log.txt");
        }

        private Mapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<DateTime, long>().ConvertUsing(new DateTimeTypeConverter());
                cfg.CreateMap<long, DateTime>().ConvertUsing(new DateTimeTypeConverterReverse());
                cfg.CreateMap<TimeSpan, long>().ConvertUsing(new TimeSpanTypeConverter());
                cfg.CreateMap<long, TimeSpan>().ConvertUsing(new TimeSpanTypeConverterReverse());

            });
            return new Mapper(config);
        }

        private void ConfigureRabbitMq(IServiceCollection services)
        {
            var config = new RabbitMqConfig();
            Configuration.GetSection("RabbitMq").Bind(config);
            var rabbitMqFactory = RabbitMqFactory.Create(config);

            AddAwaiter(services, rabbitMqFactory);
            AddPublishing(services, rabbitMqFactory);
        }

        private void AddAwaiter(IServiceCollection services, RabbitMqFactory factory)
        {
            var consumer = factory.CreateConsumer(Queues.APIGateway);
            var awaiter = new EventsAwaiter();
            awaiter.BindConsumer(consumer);
            services.AddSingleton(awaiter);
        }

        private void AddPublishing(IServiceCollection services, RabbitMqFactory factory)
        {
            //TODO przepisac na builder
            var publishers = new Dictionary<string, IPublisher>();
            publishers.Add(Queues.Accounts, factory.CreatePublisher(Queues.Accounts));
            publishers.Add(Queues.Transactions, factory.CreatePublisher(Queues.Transactions));
            services.AddSingleton(new PublishingRouter(publishers));
        }
    }
}

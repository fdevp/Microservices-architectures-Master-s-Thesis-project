﻿using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SharedClasses;
using SharedClasses.Commands;
using TransactionsWriteMicroservice.Repository;

namespace TransactionsWriteMicroservice
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(c => c.AddSerilog().AddFile("log.txt"));
            var commandsRepository = new CommandsRepository();
            services.AddSingleton(commandsRepository);
            services.AddGrpc(options =>
            {
                options.Interceptors.Add<LoggingInterceptor>("TransactionsWrite");
                options.Interceptors.Add<CommandsInterceptor>(commandsRepository);
                options.MaxReceiveMessageSize = 500 * 1024 * 1024;
            });
            services.AddSingleton(CreateMapper());
            services.AddSingleton(new TransactionsRepository());
            services.AddRabbitMqPublisher(configuration, "TransactionsWrite");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<TransactionsWriteService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }

        private Mapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddGrpcConverters();
                cfg.CreateMap<Transaction, Repository.Transaction>().ReverseMap();
            });
            return new Mapper(config);
        }
    }
}

﻿using System.Net.Http;
using AccountsWriteMicroservice.Repository;
using AutoMapper;
using Grpc.Net.Client;
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
using SharedClasses.Messaging;
using static TransactionsWriteMicroservice.TransactionsWrite;

namespace AccountsWriteMicroservice
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
                options.Interceptors.Add<LoggingInterceptor>("AccountsWrite");
                //options.Interceptors.Add<CommandsInterceptor>(commandsRepository);
                options.MaxReceiveMessageSize = 16 * 1024 * 1024;
            });

            services.AddSingleton(CreateMapper());
            services.AddSingleton(CreateTransactionsClient());
            services.AddSingleton(new AccountsRepository());
            services.AddRabbitMqPublisher(configuration, "AccountsWrite");
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
                endpoints.MapGrpcService<AccountsWriteService>();

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
                cfg.CreateMap<Account, Models.Account>().ReverseMap();
            });
            return new Mapper(config);
        }

        private TransactionsWriteClient CreateTransactionsClient()
        {
            var addresses = new EndpointsAddresses();
            configuration.GetSection("Addresses").Bind(addresses);

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var httpClient = new HttpClient(httpClientHandler);
            var channel = GrpcChannel.ForAddress(addresses.TransactionsWrite, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 16 * 1024 * 1024 });

            return new TransactionsWriteClient(channel);
        }
    }
}

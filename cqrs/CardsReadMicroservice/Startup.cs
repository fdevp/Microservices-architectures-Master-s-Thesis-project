﻿using System.Net.Http;
using AutoMapper;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedClasses;
using Microsoft.Extensions.Logging;
using CardsReadMicroservice.Repository;
using static AccountsReadMicroservice.AccountsRead;
using Microsoft.Extensions.Configuration;
using SharedClasses.Messaging;

namespace CardsReadMicroservice
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
            var repository = new CardsRepository();

            services.AddGrpc(options =>
            {
                options.Interceptors.Add<LoggingInterceptor>("Cards");
            });
            services.AddSingleton(CreateMapper());
            services.AddSingleton(CreateAccountsClient());
            services.AddSingleton(repository);
            SetProjectionListener(repository);
        }

        private void SetProjectionListener(CardsRepository repository)
        {
            var config = new RabbitMqConfig();
            configuration.GetSection("RabbitMq").Bind(config);
            var rabbitMq = new RabbitMqChannelFactory().CreateReadChannel<CardsUpsert, CardsRemove>(config);

            rabbitMq.Received += (sender, projection) =>
            {
                if (projection.Upsert != null)
                    repository.Upsert(projection.Upsert);
                if (projection.Remove != null)
                    repository.Remove(projection.Remove);
            };
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<CardsReadService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });

            loggerFactory.AddFile("log.txt");
        }

        private Mapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Card, Repository.Card>().ReverseMap();
                cfg.CreateMap<Block, Repository.Block>().ReverseMap();
            });
            return new Mapper(config);
        }

        private AccountsReadClient CreateAccountsClient()
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var httpClient = new HttpClient(httpClientHandler);
            var channel = GrpcChannel.ForAddress("https://localhost:5022", new GrpcChannelOptions { HttpClient = httpClient });
            return new AccountsReadClient(channel);
        }
    }
}
﻿using System.Net.Http;
using AutoMapper;
using Grpc.Net.Client;
using LoansMicroservice.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedClasses;
using static PaymentsMicroservice.Payments;
using static TransactionsMicroservice.Transactions;

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
            services.AddGrpc(options =>
            {
                options.Interceptors.Add<LoggingInterceptor>("Loans");
                options.MaxReceiveMessageSize = 16 * 1024 * 1024;
            });
            services.AddSingleton(CreateMapper());
            services.AddSingleton(new LoansRepository());
            CreateClients(services);
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
                endpoints.MapGrpcService<LoansService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });

            loggerFactory.AddFile("log-loans.txt");
        }

        private Mapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddGrpcConverters();
                cfg.CreateMap<Loan, Repository.Loan>().ReverseMap();
            });
            return new Mapper(config);
        }

        private void CreateClients(IServiceCollection services)
        {
            var addresses = new EndpointsAddresses();
            Configuration.GetSection("Addresses").Bind(addresses);

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var httpClient = new HttpClient(httpClientHandler);

            var transactionsChannel = GrpcChannel.ForAddress(addresses.Transactions, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 16 * 1024 * 1024 });
            services.AddSingleton(new TransactionsClient(transactionsChannel));

            var paymentsChannel = GrpcChannel.ForAddress(addresses.Payments, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 16 * 1024 * 1024 });
            services.AddSingleton(new PaymentsClient(paymentsChannel));
        }
    }
}

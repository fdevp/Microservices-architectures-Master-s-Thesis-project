using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedClasses;
using Microsoft.Extensions.Logging;
using PaymentsReadMicroservice.Repository;
using static LoansReadMicroservice.LoansRead;
using static TransactionsReadMicroservice.TransactionsRead;
using Microsoft.Extensions.Configuration;
using SharedClasses.Messaging;
using Serilog;

namespace PaymentsReadMicroservice
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
            var repository = new PaymentsRepository();
            services.AddGrpc(options =>
            {
                options.Interceptors.Add<LoggingInterceptor>("PaymentsRead");
                options.MaxReceiveMessageSize = 500 * 1024 * 1024;
            });
            services.AddSingleton(CreateMapper());
            services.AddSingleton(repository);
            CreateClients(services);
            SetProjectionListener(repository, services);
        }

        private void SetProjectionListener(PaymentsRepository repository, IServiceCollection services)
        {
            var config = new RabbitMqConfig();
            configuration.GetSection("RabbitMq").Bind(config);

            var logger = services.BuildServiceProvider().GetService<ILogger<RabbitMqPublisher>>();
            var rabbitMq = new RabbitMqChannelFactory().CreateReadChannel<Repository.Payment, string>(config, "PaymentsRead", logger);

            rabbitMq.Received += (sender, projection) =>
            {
                if (projection.Upsert != null)
                    repository.Upsert(projection.Upsert);
                if (projection.Remove != null)
                    repository.Remove(projection.Remove);
            };
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
                endpoints.MapGrpcService<PaymentsReadService>();

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
                cfg.CreateMap<PaymentStatus, Repository.PaymentStatus>().ReverseMap();
                cfg.CreateMap<Payment, Repository.Payment>().ReverseMap();
            });
            return new Mapper(config);
        }

        private void CreateClients(IServiceCollection services)
        {
            var addresses = new EndpointsAddresses();
            configuration.GetSection("Addresses").Bind(addresses);

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var httpClient = new HttpClient(httpClientHandler);

            var transactionsChannel = GrpcChannel.ForAddress(addresses.TransactionsRead, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 500 * 1024 * 1024 });
            services.AddSingleton(new TransactionsReadClient(transactionsChannel));

            var loansChannel = GrpcChannel.ForAddress(addresses.LoansRead, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 500 * 1024 * 1024 });
            services.AddSingleton(new LoansReadClient(loansChannel));
        }
    }
}

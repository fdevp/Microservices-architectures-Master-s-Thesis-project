using System.Net.Http;
using AutoMapper;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedClasses;
using Microsoft.Extensions.Logging;
using PaymentsWriteMicroservice.Repository;
using static LoansWriteMicroservice.LoansWrite;
using static TransactionsWriteMicroservice.TransactionsWrite;
using Microsoft.Extensions.Configuration;
using SharedClasses.Messaging;

namespace PaymentsWriteMicroservice
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
            services.AddGrpc(options =>
            {
                options.Interceptors.Add<LoggingInterceptor>("Payments");
            });
            services.AddSingleton(CreateMapper());
            services.AddSingleton(new PaymentsRepository());
            services.AddRabbitMqPublisher(configuration);
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
                endpoints.MapGrpcService<PaymentsWriteService>();

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
                cfg.CreateMap<PaymentStatus, Repository.PaymentStatus>().ReverseMap();
                cfg.CreateMap<Payment, Repository.Payment>().ReverseMap();
            });
            return new Mapper(config);
        }

        private void CreateClients(IServiceCollection services)
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var httpClient = new HttpClient(httpClientHandler);

            var transactionsChannel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions { HttpClient = httpClient });
            services.AddSingleton(new TransactionsWriteClient(transactionsChannel));

            var loansChannel = GrpcChannel.ForAddress("https://localhost:5015", new GrpcChannelOptions { HttpClient = httpClient });
            services.AddSingleton(new LoansWriteClient(loansChannel));
        }
    }
}

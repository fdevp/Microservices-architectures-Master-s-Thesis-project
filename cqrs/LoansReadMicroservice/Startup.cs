using System.Net.Http;
using AutoMapper;
using Grpc.Net.Client;
using LoansReadMicroservice.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedClasses;
using SharedClasses.Messaging;
using static PaymentsReadMicroservice.PaymentsRead;

namespace LoansReadMicroservice
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
            var repository = new LoansRepository();
            services.AddGrpc(options =>
            {
                options.Interceptors.Add<LoggingInterceptor>("Loans");
            });
            services.AddSingleton(CreateMapper());
            services.AddSingleton(CreatePaymentsClient());
            services.AddSingleton(repository);

            var config = new RabbitMqConfig();
            configuration.GetSection("RabbitMq").Bind(config);
            var rabbitMq = new RabbitMqChannelFactory().CreateReadChannel<Repository.Loan, string>(config);
            SetProjectionListener(rabbitMq, repository);
        }

        private void SetProjectionListener(RabbitMqConsumer<Repository.Loan, string> consumer, LoansRepository repository)
        {
            consumer.Received += (sender, projection) =>
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
                endpoints.MapGrpcService<LoansReadService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });

            loggerFactory.AddFile("log.txt");
        }

        private Mapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Loan, Repository.Loan>().ReverseMap());
            return new Mapper(config);
        }

        private PaymentsReadClient CreatePaymentsClient()
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var httpClient = new HttpClient(httpClientHandler);
            var channel = GrpcChannel.ForAddress("https://localhost:5013", new GrpcChannelOptions { HttpClient = httpClient });
            return new PaymentsReadClient(channel);
        }
    }
}

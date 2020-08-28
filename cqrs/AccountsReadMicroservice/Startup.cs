using System.Net.Http;
using AccountsReadMicroservice.Repository;
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
using SharedClasses.Messaging;
using static TransactionsReadMicroservice.TransactionsRead;

namespace AccountsReadMicroservice
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
            var failureSettings = new FailureSettings();
            configuration.GetSection("FailureSettings").Bind(failureSettings);

            services.AddLogging(c => c.AddSerilog().AddFile("log.txt"));
            var repository = new AccountsRepository();
            services.AddGrpc(options =>
            {
                options.Interceptors.Add<LoggingInterceptor>("AccountsRead", failureSettings);
                options.MaxReceiveMessageSize = 16 * 1024 * 1024;
            });
            services.AddSingleton(CreateMapper());
            services.AddSingleton(CreateTransactionsClient());
            services.AddSingleton(repository);
            SetProjectionListener(repository, services);
        }

        public void SetProjectionListener(AccountsRepository repository, IServiceCollection services)
        {
            var config = new RabbitMqConfig();
            configuration.GetSection("RabbitMq").Bind(config);

            var logger = services.BuildServiceProvider().GetService<ILogger<RabbitMqPublisher>>();
            var rabbitMq = new RabbitMqChannelFactory().CreateReadChannel<Models.Account, string>(config, "AccountsRead", logger);

            rabbitMq.Received += (sender, projection) =>
            {
                if (projection.Upsert != null && projection.Upsert.Length > 0)
                    repository.Upsert(projection.Upsert);
                if (projection.Upsert != null && projection.Upsert.Length == 0)
                    repository.Clear();
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
                endpoints.MapGrpcService<AccountsReadService>();

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

        private TransactionsReadClient CreateTransactionsClient()
        {
            var addresses = new EndpointsAddresses();
            configuration.GetSection("Addresses").Bind(addresses);

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var httpClient = new HttpClient(httpClientHandler);
            var channel = GrpcChannel.ForAddress(addresses.TransactionsRead, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 16 * 1024 * 1024 });
            return new TransactionsReadClient(channel);
        }
    }
}

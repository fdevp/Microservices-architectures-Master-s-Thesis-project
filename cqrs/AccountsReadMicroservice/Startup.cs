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
            var repository = new AccountsRepository();

            services.AddGrpc(options =>
            {
                options.Interceptors.Add<LoggingInterceptor>("Accounts");
            });
            services.AddSingleton(CreateMapper());
            services.AddSingleton(CreateTransactionsClient());
            services.AddSingleton(repository);
            SetProjectionListener(repository);
        }

        public void SetProjectionListener(AccountsRepository repository)
        {
            var config = new RabbitMqConfig();
            configuration.GetSection("RabbitMq").Bind(config);
            var rabbitMq = new RabbitMqChannelFactory().CreateReadChannel<Repository.Account, string>(config);

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
                endpoints.MapGrpcService<AccountsReadService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });

            loggerFactory.AddFile("log.txt");
        }

        private Mapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Account, Repository.Account>().ReverseMap());
            return new Mapper(config);
        }

        private TransactionsReadClient CreateTransactionsClient()
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var httpClient = new HttpClient(httpClientHandler);
            var channel = GrpcChannel.ForAddress("https://localhost:5001", new GrpcChannelOptions { HttpClient = httpClient });
            return new TransactionsReadClient(channel);
        }
    }
}

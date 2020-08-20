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
using CardsWriteMicroservice.Repository;
using static AccountsWriteMicroservice.AccountsWrite;
using Microsoft.Extensions.Configuration;
using SharedClasses.Messaging;
using SharedClasses.Commands;
using Serilog;

namespace CardsWriteMicroservice
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
                options.Interceptors.Add<LoggingInterceptor>("CardsWrite");
                options.Interceptors.Add<CommandsInterceptor>(commandsRepository);
                options.MaxReceiveMessageSize = 500 * 1024 * 1024;
            });
            services.AddSingleton(CreateMapper());
            services.AddSingleton(CreateAccountsClient());
            services.AddSingleton(new CardsRepository());
            services.AddRabbitMqPublisher(configuration, "CardsWrite");
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
                endpoints.MapGrpcService<CardsWriteService>();

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
                cfg.CreateMap<Card, Models.Card>().ReverseMap();
                cfg.CreateMap<Block, Models.Block>().ReverseMap();
            });
            return new Mapper(config);
        }

        private AccountsWriteClient CreateAccountsClient()
        {
            var addresses = new EndpointsAddresses();
            configuration.GetSection("Addresses").Bind(addresses);

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var httpClient = new HttpClient(httpClientHandler);
            var channel = GrpcChannel.ForAddress(addresses.AccountsWrite, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 500 * 1024 * 1024 });
            return new AccountsWriteClient(channel);
        }
    }
}

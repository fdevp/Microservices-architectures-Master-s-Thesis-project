using System.Net.Http;
using AutoMapper;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static AccountsMicroservice.Accounts;
using SharedClasses;
using Microsoft.Extensions.Logging;
using CardsMicroservice.Repository;
using Microsoft.Extensions.Configuration;
using static TransactionsMicroservice.Transactions;

namespace CardsMicroservice
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
                options.Interceptors.Add<LoggingInterceptor>("Cards");
            });
            services.AddSingleton(CreateMapper());
            services.AddSingleton(new CardsRepository());
            ConfigureGrpcConnections(services);
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
                endpoints.MapGrpcService<CardsService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });

            loggerFactory.AddFile("log-cards.txt");
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

        private void ConfigureGrpcConnections(IServiceCollection services)
        {
            var addresses = new EndpointsAddresses();
            Configuration.GetSection("Addresses").Bind(addresses);

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            var httpClient = new HttpClient(httpClientHandler);
            var accountsChannel = GrpcChannel.ForAddress(addresses.Accounts, new GrpcChannelOptions { HttpClient = httpClient });
            services.AddSingleton(new AccountsClient(accountsChannel));

            var transactionsChannel = GrpcChannel.ForAddress(addresses.Transactions, new GrpcChannelOptions { HttpClient = httpClient });
            services.AddSingleton(new TransactionsClient(transactionsChannel));
        }
    }
}

using System.Net.Http;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedClasses;
using static AccountsReadMicroservice.AccountsRead;
using static CardsReadMicroservice.CardsRead;
using static LoansReadMicroservice.LoansRead;
using static PaymentsReadMicroservice.PaymentsRead;
using static TransactionsReadMicroservice.TransactionsRead;

namespace ReportsMicroservice
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
                options.Interceptors.Add<LoggingInterceptor>("Reports");
            });
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
                endpoints.MapGrpcService<ReportsService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });

            loggerFactory.AddFile("log.txt");
        }

        private void CreateClients(IServiceCollection services)
        {
            var addresses = new EndpointsAddresses();
            Configuration.GetSection("Addresses").Bind(addresses);

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var httpClient = new HttpClient(httpClientHandler);

            var transactionsChannel = GrpcChannel.ForAddress("", new GrpcChannelOptions { HttpClient = httpClient });
            services.AddSingleton(new TransactionsReadClient(transactionsChannel));

            var accountsChannel = GrpcChannel.ForAddress("", new GrpcChannelOptions { HttpClient = httpClient });
            services.AddSingleton(new AccountsReadClient(accountsChannel));

            var paymentsChannel = GrpcChannel.ForAddress("", new GrpcChannelOptions { HttpClient = httpClient });
            services.AddSingleton(new PaymentsReadClient(paymentsChannel));

            var loansChannel = GrpcChannel.ForAddress("", new GrpcChannelOptions { HttpClient = httpClient });
            services.AddSingleton(new LoansReadClient(loansChannel));

            var cardsChannel = GrpcChannel.ForAddress("", new GrpcChannelOptions { HttpClient = httpClient });
            services.AddSingleton(new CardsReadClient(loansChannel));
        }
    }
}

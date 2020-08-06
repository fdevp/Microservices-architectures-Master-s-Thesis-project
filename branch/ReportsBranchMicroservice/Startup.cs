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
using static AccountsMicroservice.Accounts;
using static CardsMicroservice.Cards;
using static LoansMicroservice.Loans;
using static PaymentsMicroservice.Payments;
using static TransactionsMicroservice.Transactions;

namespace ReportsBranchMicroservice
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(options =>
            {
                options.Interceptors.Add<LoggingInterceptor>("Reports_branch");
                options.MaxReceiveMessageSize = 8 * 1024 * 1024;
            });
            CreateClients(services);
            services.AddSingleton<DataFetcher>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<ReportsBranchService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });

            loggerFactory.AddFile("log-reports.txt");
        }

        private void CreateClients(IServiceCollection services)
        {
            var addresses = new EndpointsAddresses();
            Configuration.GetSection("Addresses").Bind(addresses);

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var httpClient = new HttpClient(httpClientHandler);

            var transactionsChannel = GrpcChannel.ForAddress(addresses.Transactions, new GrpcChannelOptions { HttpClient = httpClient,  MaxReceiveMessageSize = 8 * 1024 * 1024 });
            services.AddSingleton(new TransactionsClient(transactionsChannel));

            var accountsChannel = GrpcChannel.ForAddress(addresses.Accounts, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 8 * 1024 * 1024 });
            services.AddSingleton(new AccountsClient(accountsChannel));

            var paymentsChannel = GrpcChannel.ForAddress(addresses.Payments, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 8 * 1024 * 1024 });
            services.AddSingleton(new PaymentsClient(paymentsChannel));

            var loansChannel = GrpcChannel.ForAddress(addresses.Loans, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 8 * 1024 * 1024 });
            services.AddSingleton(new LoansClient(loansChannel));

            var cardsChannel = GrpcChannel.ForAddress(addresses.Cards, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 8 * 1024 * 1024 });
            services.AddSingleton(new CardsClient(cardsChannel));
        }
    }
}

using System.Net.Http;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedClasses;
using static AccountsMicroservice.Accounts;
using static CardsMicroservice.Cards;
using static LoansMicroservice.Loans;
using static PaymentsMicroservice.Payments;
using static TransactionsMicroservice.Transactions;

namespace PanelsBranchMicroservice
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(options =>
            {
                options.Interceptors.Add<LoggingInterceptor>("Panels_branch");
            });
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
                endpoints.MapGrpcService<PanelsBranchService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });

            loggerFactory.AddFile("log.txt");
        }

        private void ConfigureGrpcConnections(IServiceCollection services)
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var httpClient = new HttpClient(httpClientHandler);

            services.AddSingleton(new TransactionsClient(CreateChannel(httpClient, "localhost", "5001")));
            services.AddSingleton(new AccountsClient(CreateChannel(httpClient, "localhost", "5011")));
            services.AddSingleton(new PaymentsClient(CreateChannel(httpClient, "localhost", "5013")));
            services.AddSingleton(new CardsClient(CreateChannel(httpClient, "localhost", "5014")));
            services.AddSingleton(new LoansClient(CreateChannel(httpClient, "localhost", "5015")));
        }

        private GrpcChannel CreateChannel(HttpClient httpClient, string host, string port)
        {
            var address = $"https://{host}:{port}";
            var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions { HttpClient = httpClient });
            return channel;
        }
    }
}

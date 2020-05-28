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
using static LoansMicroservice.Loans;
using static PaymentsMicroservice.Payments;
using static UsersMicroservice.Users;

namespace BatchesBranchMicroservice
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(options =>
            {
                options.Interceptors.Add<LoggingInterceptor>("Batches_branch");
            });
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
                endpoints.MapGrpcService<BatchesBranchService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });

            loggerFactory.AddFile("log.txt");
        }

        private void CreateClients(IServiceCollection services)
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var httpClient = new HttpClient(httpClientHandler);

            var accountsChannel = GrpcChannel.ForAddress("https://localhost:5011", new GrpcChannelOptions { HttpClient = httpClient });
            services.AddSingleton(new AccountsClient(accountsChannel));

            var usersChannel = GrpcChannel.ForAddress("https://localhost:5012", new GrpcChannelOptions { HttpClient = httpClient });
            services.AddSingleton(new UsersClient(usersChannel));

            var paymentsChannel = GrpcChannel.ForAddress("https://localhost:5013", new GrpcChannelOptions { HttpClient = httpClient });
            services.AddSingleton(new PaymentsClient(paymentsChannel));

            var loansChannel = GrpcChannel.ForAddress("https://localhost:5015", new GrpcChannelOptions { HttpClient = httpClient });
            services.AddSingleton(new LoansClient(loansChannel));
        }
    }
}

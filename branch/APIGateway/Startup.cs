using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static AccountsMicroservice.Accounts;
using static BatchesBranchMicroservice.BatchesBranch;
using static CardsMicroservice.Cards;
using static LoansMicroservice.Loans;
using static PanelsBranchMicroservice.PanelsBranch;
using static PaymentsMicroservice.Payments;
using static ReportsBranchMicroservice.ReportsBranch;
using static TransactionsMicroservice.Transactions;
using static UsersMicroservice.Users;

namespace APIGateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void ConfigureGrpcConnections(IServiceCollection services)
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var httpClient = new HttpClient(httpClientHandler);


            services.AddSingleton(new TransactionsClient(CreateChannel(httpClient, "localhost", "5001")));
            services.AddSingleton(new AccountsClient(CreateChannel(httpClient, "localhost", "5011")));
            services.AddSingleton(new UsersClient(CreateChannel(httpClient, "localhost", "5012")));
            services.AddSingleton(new PaymentsClient(CreateChannel(httpClient, "localhost", "5013")));
            services.AddSingleton(new CardsClient(CreateChannel(httpClient, "localhost", "5014")));
            services.AddSingleton(new LoansClient(CreateChannel(httpClient, "localhost", "5015")));

            services.AddSingleton(new ReportsBranchClient(CreateChannel(httpClient, "localhost", "5011")));
            services.AddSingleton(new PanelsBranchClient(CreateChannel(httpClient, "localhost", "5012")));
            services.AddSingleton(new BatchesBranchClient(CreateChannel(httpClient, "localhost", "5013")));
        }

        private GrpcChannel CreateChannel(HttpClient httpClient, string host, string port)
        {
            var address = $"https://{host}:{port}";
            var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions { HttpClient = httpClient });
            return channel;
        }
    }
}

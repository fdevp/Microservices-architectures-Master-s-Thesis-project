
using System.Linq;
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
using static TransactionsReadMicroservice.TransactionsRead;
using static TransactionsWriteMicroservice.TransactionsWrite;

namespace TransactionsLoadBalancer
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
                options.Interceptors.Add<LoggingInterceptor>("TransactionsLoadBalancer");
                options.MaxReceiveMessageSize = 8 * 1024 * 1024;
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
                endpoints.MapGrpcService<TransactionsReadBalancingService>();
                endpoints.MapGrpcService<TransactionsWriteBalancingService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });

            loggerFactory.AddFile("log-transactionsLoadBalancer.txt");
        }

        public void CreateClients(IServiceCollection services)
        {
            var readAddresses = Configuration.GetSection("ReadServices").GetChildren().Select(v => v.Value).Distinct();
            var writeAddresses = Configuration.GetSection("WriteServices").GetChildren().Select(v => v.Value).Distinct();

            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            var httpClient = new HttpClient(httpClientHandler);

            foreach (var address in readAddresses)
            {
                var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 8 * 1024 * 1024 });
                services.AddSingleton(new TransactionsReadClient(channel));
            }
            
            foreach (var address in writeAddresses)
            {
                var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 8 * 1024 * 1024 });
                services.AddSingleton(new TransactionsWriteClient(channel));
            }
        }
    }
}

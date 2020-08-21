using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharedClasses;
using TransactionsMicroservice.Repository;

namespace TransactionsMicroservice
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
            var failureSettings = new FailureSettings();
            Configuration.GetSection("FailureSettings").Bind(failureSettings);

            services.AddGrpc(options =>
            {
                options.Interceptors.Add<LoggingInterceptor>("Transactions", failureSettings);
                options.MaxReceiveMessageSize = 8 * 1024 * 1024;
            });
            services.AddSingleton(CreateMapper());
            services.AddSingleton(new TransactionsRepository());
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
                endpoints.MapGrpcService<TransactionsService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });

            loggerFactory.AddFile("log-transactions.txt");
        }

        private Mapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddGrpcConverters();
                cfg.CreateMap<Transaction, Repository.Transaction>().ReverseMap();
            });
            return new Mapper(config);
        }
    }
}

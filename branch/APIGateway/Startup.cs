using System;
using System.Net.Http;
using AccountsMicroservice;
using APIGateway.Middlewares;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using CardsMicroservice;
using Google.Protobuf.Collections;
using Grpc.Net.Client;
using LoansMicroservice;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentsMicroservice;
using UsersMicroservice;
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
            services.AddControllers().AddNewtonsoftJson();
            services.AddSingleton(CreateMapper());
            ConfigureGrpcConnections(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseMiddleware<LoggingMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            loggerFactory.AddFile("log.txt");
        }

        private Mapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.ForAllPropertyMaps(
                    map => map.DestinationType.IsGenericType && map.DestinationType.GetGenericTypeDefinition() == typeof(RepeatedField<>),
                    (map, options) => options.UseDestinationValue());
                cfg.CreateMap<DateTime, long>().ConvertUsing(new DateTimeTypeConverter());
                cfg.CreateMap<long, DateTime>().ConvertUsing(new DateTimeTypeConverterReverse());
                cfg.CreateMap<TimeSpan, long>().ConvertUsing(new TimeSpanTypeConverter());
                
                cfg.CreateMap<Transaction, TransactionDTO>().ReverseMap();
                cfg.CreateMap<Account, AccountDTO>().ReverseMap();
                cfg.CreateMap<Card, CardDTO>().ReverseMap();
                cfg.CreateMap<Payment, PaymentDTO>().ReverseMap();
                cfg.CreateMap<User, UserDTO>().ReverseMap();
                cfg.CreateMap<Loan, LoanDTO>().ReverseMap();

                cfg.CreateMap<TransactionsMicroservice.SetupRequest, TransactionsSetup>().ReverseMap();
                cfg.CreateMap<AccountsMicroservice.SetupRequest, AccountsSetup>().ReverseMap();
                cfg.CreateMap<CardsMicroservice.SetupRequest, CardsSetup>().ReverseMap();
                cfg.CreateMap<PaymentsMicroservice.SetupRequest, PaymentsSetup>().ReverseMap();
                cfg.CreateMap<UsersMicroservice.SetupRequest, UsersSetup>().ReverseMap();
                cfg.CreateMap<LoansMicroservice.SetupRequest, LoansSetup>().ReverseMap();

                cfg.CreateMap<Transfer, AccountTransfer>().ReverseMap();
                cfg.CreateMap<CardsMicroservice.TransferRequest, CardTransfer>().ReverseMap();
                cfg.CreateMap<UsersMicroservice.SignInRequest, TokenRequest>().ReverseMap();


            });
            return new Mapper(config);
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

            services.AddSingleton(new ReportsBranchClient(CreateChannel(httpClient, "localhost", "5021")));
            services.AddSingleton(new PanelsBranchClient(CreateChannel(httpClient, "localhost", "5022")));
            services.AddSingleton(new BatchesBranchClient(CreateChannel(httpClient, "localhost", "5023")));
        }

        private GrpcChannel CreateChannel(HttpClient httpClient, string host, string port)
        {
            var address = $"https://{host}:{port}";
            var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions { HttpClient = httpClient });
            return channel;
        }
    }
}

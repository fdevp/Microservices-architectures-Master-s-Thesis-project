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
using PanelsBranchMicroservice;
using PaymentsMicroservice;
using SharedClasses;
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

            app.UseMiddleware<FlowIdMiddleware>();
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
                cfg.CreateMap<long, TimeSpan>().ConvertUsing(new TimeSpanTypeConverterReverse());

                cfg.CreateMap<Transaction, TransactionDTO>().ReverseMap()
                    .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => src.PaymentId == null ? "" : src.PaymentId))
                    .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId == null ? "" : src.CardId));

                cfg.CreateMap<Account, AccountDTO>().ReverseMap();
                cfg.CreateMap<Card, CardDTO>().ReverseMap();
                cfg.CreateMap<Payment, PaymentDTO>().ReverseMap();
                cfg.CreateMap<User, UserDTO>().ReverseMap();
                cfg.CreateMap<Loan, LoanDTO>().ReverseMap();
                cfg.CreateMap<Message, MessageDTO>().ReverseMap();
                cfg.CreateMap<AccountBalance, BalanceDTO>().ForMember(balance => balance.Amount, m => m.MapFrom(dto => dto.Balance));

                cfg.CreateMap<TransactionsMicroservice.SetupRequest, TransactionsSetup>().ReverseMap();
                cfg.CreateMap<AccountsMicroservice.SetupRequest, AccountsSetup>().ReverseMap();
                cfg.CreateMap<CardsMicroservice.SetupRequest, CardsSetup>().ReverseMap();
                cfg.CreateMap<PaymentsMicroservice.SetupRequest, PaymentsSetup>().ReverseMap();
                cfg.CreateMap<UsersMicroservice.SetupRequest, UsersSetup>().ReverseMap();
                cfg.CreateMap<LoansMicroservice.SetupRequest, LoansSetup>().ReverseMap();

                cfg.CreateMap<Transfer, AccountTransfer>().ReverseMap();
                cfg.CreateMap<CardsMicroservice.TransferRequest, CardTransfer>().ReverseMap();
                cfg.CreateMap<UsersMicroservice.SignInRequest, TokenRequest>().ReverseMap();

                cfg.CreateMap<GetPanelResponse, Panel>();
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


            services.AddSingleton(new TransactionsClient(GrpcChannel.ForAddress(addresses.Transactions, new GrpcChannelOptions { HttpClient = httpClient })));
            services.AddSingleton(new AccountsClient(GrpcChannel.ForAddress(addresses.Accounts, new GrpcChannelOptions { HttpClient = httpClient })));
            services.AddSingleton(new UsersClient(GrpcChannel.ForAddress(addresses.Users, new GrpcChannelOptions { HttpClient = httpClient })));
            services.AddSingleton(new PaymentsClient(GrpcChannel.ForAddress(addresses.Payments, new GrpcChannelOptions { HttpClient = httpClient })));
            services.AddSingleton(new CardsClient(GrpcChannel.ForAddress(addresses.Cards, new GrpcChannelOptions { HttpClient = httpClient })));
            services.AddSingleton(new LoansClient(GrpcChannel.ForAddress(addresses.Loans, new GrpcChannelOptions { HttpClient = httpClient })));

            services.AddSingleton(new ReportsBranchClient(GrpcChannel.ForAddress(addresses.ReportsBranch, new GrpcChannelOptions { HttpClient = httpClient })));
            services.AddSingleton(new PanelsBranchClient(GrpcChannel.ForAddress(addresses.PanelBranch, new GrpcChannelOptions { HttpClient = httpClient })));
            services.AddSingleton(new BatchesBranchClient(GrpcChannel.ForAddress(addresses.BatchBranch, new GrpcChannelOptions { HttpClient = httpClient })));
        }
    }
}

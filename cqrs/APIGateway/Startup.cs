using System;
using System.Net.Http;
using APIGateway.Middlewares;
using APIGateway.Models;
using APIGateway.Models.Setup;
using APIGateway.Reports;
using AutoMapper;
using Google.Protobuf.Collections;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SharedClasses;
using UsersMicroservice;
using static AccountsReadMicroservice.AccountsRead;
using static AccountsWriteMicroservice.AccountsWrite;
using static CardsReadMicroservice.CardsRead;
using static CardsWriteMicroservice.CardsWrite;
using static LoansReadMicroservice.LoansRead;
using static LoansWriteMicroservice.LoansWrite;
using static PaymentsReadMicroservice.PaymentsRead;
using static PaymentsWriteMicroservice.PaymentsWrite;
using static TransactionsReadMicroservice.TransactionsRead;
using static TransactionsWriteMicroservice.TransactionsWrite;
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
            services.AddLogging(c => c.AddSerilog().AddFile("log.txt"));
            services.AddControllers().AddNewtonsoftJson();
            services.AddSingleton(CreateMapper());
            ConfigureGrpcConnections(services);
            services.AddSingleton<ReportDataFetcher>();
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

            app.UseMiddleware<FlowIdMiddleware>();
            app.UseMiddleware<LoggingMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private Mapper CreateMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.ForAllPropertyMaps(
                    map => map.DestinationType.IsGenericType && map.DestinationType.GetGenericTypeDefinition() == typeof(RepeatedField<>),
                    (map, options) => options.UseDestinationValue());
                
                cfg.AddGrpcConverters();

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

                cfg.CreateMap<UsersMicroservice.SetupRequest, UsersSetup>().ReverseMap();
                cfg.CreateMap<TransactionsWriteMicroservice.SetupRequest, TransactionsSetup>().ReverseMap();
                cfg.CreateMap<AccountsWriteMicroservice.SetupRequest, AccountsSetup>().ReverseMap();
                cfg.CreateMap<CardsWriteMicroservice.SetupRequest, CardsSetup>().ReverseMap();
                cfg.CreateMap<PaymentsWriteMicroservice.SetupRequest, PaymentsSetup>().ReverseMap();
                cfg.CreateMap<LoansWriteMicroservice.SetupRequest, LoansSetup>().ReverseMap();

                cfg.CreateMap<Transfer, AccountTransfer>()
                    .ForMember(dest => dest.PaymentId, opt => opt.MapFrom(src => src.PaymentId == null ? "" : src.PaymentId))
                    .ReverseMap();
                cfg.CreateMap<CardsWriteMicroservice.TransferRequest, CardTransfer>().ReverseMap();
                cfg.CreateMap<UsersMicroservice.SignInRequest, TokenRequest>().ReverseMap();
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

            services.AddSingleton(new TransactionsWriteClient(GrpcChannel.ForAddress(addresses.TransactionsWrite, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 16 * 1024 * 1024 })));
            services.AddSingleton(new TransactionsReadClient(GrpcChannel.ForAddress(addresses.TransactionsRead, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 16 * 1024 * 1024 })));

            services.AddSingleton(new AccountsWriteClient(GrpcChannel.ForAddress(addresses.AccountsWrite, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 16 * 1024 * 1024 })));
            services.AddSingleton(new AccountsReadClient(GrpcChannel.ForAddress(addresses.AccountsRead, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 16 * 1024 * 1024 })));

            services.AddSingleton(new PaymentsWriteClient(GrpcChannel.ForAddress(addresses.PaymentsWrite, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 16 * 1024 * 1024 })));
            services.AddSingleton(new PaymentsReadClient(GrpcChannel.ForAddress(addresses.PaymentsRead, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 16 * 1024 * 1024 })));

            services.AddSingleton(new CardsWriteClient(GrpcChannel.ForAddress(addresses.CardsWrite, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 16 * 1024 * 1024 })));
            services.AddSingleton(new CardsReadClient(GrpcChannel.ForAddress(addresses.CardsRead, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 16 * 1024 * 1024 })));

            services.AddSingleton(new LoansWriteClient(GrpcChannel.ForAddress(addresses.LoansWrite, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 16 * 1024 * 1024 })));
            services.AddSingleton(new LoansReadClient(GrpcChannel.ForAddress(addresses.LoansRead, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 16 * 1024 * 1024 })));

            services.AddSingleton(new UsersClient(GrpcChannel.ForAddress(addresses.Users, new GrpcChannelOptions { HttpClient = httpClient, MaxReceiveMessageSize = 16 * 1024 * 1024 })));
        }
    }
}

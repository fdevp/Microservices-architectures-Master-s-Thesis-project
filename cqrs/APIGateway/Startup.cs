using System;
using System.Net.Http;
using APIGateway.Middlewares;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using Google.Protobuf.Collections;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UsersMicroservice;
using static AccountsReadMicroservice.AccountsRead;
using static AccountsWriteMicroservice.AccountsWrite;
using static CardsReadMicroservice.CardsRead;
using static CardsWriteMicroservice.CardsWrite;
using static LoansReadMicroservice.LoansRead;
using static LoansWriteMicroservice.LoansWrite;
using static PaymentsReadMicroservice.PaymentsRead;
using static PaymentsWriteMicroservice.PaymentsWrite;
using static ReportsMicroservice.Reports;
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
                cfg.CreateMap<long, TimeSpan>().ConvertUsing(new TimeSpanTypeConverterReverse());

                cfg.CreateMap<Transaction, TransactionDTO>().ReverseMap();
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

                cfg.CreateMap<Transfer, AccountTransfer>().ReverseMap();
                cfg.CreateMap<CardsWriteMicroservice.TransferRequest, CardTransfer>().ReverseMap();
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

            services.AddSingleton(new TransactionsWriteClient(CreateChannel(httpClient, "localhost", "5011")));
            services.AddSingleton(new TransactionsReadClient(CreateChannel(httpClient, "localhost", "5012")));

            services.AddSingleton(new AccountsWriteClient(CreateChannel(httpClient, "localhost", "5021")));
            services.AddSingleton(new AccountsReadClient(CreateChannel(httpClient, "localhost", "5022")));

            services.AddSingleton(new PaymentsWriteClient(CreateChannel(httpClient, "localhost", "5031")));
            services.AddSingleton(new PaymentsReadClient(CreateChannel(httpClient, "localhost", "5032")));

            services.AddSingleton(new CardsWriteClient(CreateChannel(httpClient, "localhost", "5041")));
            services.AddSingleton(new CardsReadClient(CreateChannel(httpClient, "localhost", "5042")));

            services.AddSingleton(new LoansWriteClient(CreateChannel(httpClient, "localhost", "5051")));
            services.AddSingleton(new LoansReadClient(CreateChannel(httpClient, "localhost", "5052")));

            services.AddSingleton(new UsersClient(CreateChannel(httpClient, "localhost", "5061")));
            services.AddSingleton(new ReportsClient(CreateChannel(httpClient, "localhost", "5071")));
        }

        private GrpcChannel CreateChannel(HttpClient httpClient, string host, string port)
        {
            var address = $"https://{host}:{port}";
            var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions { HttpClient = httpClient });
            return channel;
        }
    }
}

using System;
using System.Collections.Generic;
using APIGateway.Middlewares;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using SharedClasses.Events.Accounts;
using SharedClasses.Events.Cards;
using SharedClasses.Events.Loans;
using SharedClasses.Events.Payments;
using SharedClasses.Events.Transactions;
using SharedClasses.Events.Users;
using SharedClasses.Messaging;
using SharedClasses.Messaging.RabbitMq;
using SharedClasses.Models;

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
            ConfigureRabbitMq(services);
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
                cfg.CreateMap<Transaction, TransactionDTO>().ReverseMap();
                cfg.CreateMap<Account, AccountDTO>().ReverseMap();
                cfg.CreateMap<AccountBalance, BalanceDTO>().ReverseMap();
                cfg.CreateMap<Card, CardDTO>().ReverseMap();
                cfg.CreateMap<Payment, PaymentDTO>().ReverseMap();
                cfg.CreateMap<User, UserDTO>().ReverseMap();
                cfg.CreateMap<Loan, LoanDTO>().ReverseMap();
                cfg.CreateMap<UserMessage, MessageDTO>().ReverseMap();

                cfg.CreateMap<SetupTransactionsEvent, TransactionsSetup>().ReverseMap();
                cfg.CreateMap<SetupAccountsEvent, AccountsSetup>().ReverseMap();
                cfg.CreateMap<SetupCardsEvent, CardsSetup>().ReverseMap();
                cfg.CreateMap<SetupPaymentsEvent, PaymentsSetup>().ReverseMap();
                cfg.CreateMap<SetupUsersEvent, UsersSetup>().ReverseMap();
                cfg.CreateMap<SetupLoansEvent, LoansSetup>().ReverseMap();
            });
            return new Mapper(config);
        }

        private void ConfigureRabbitMq(IServiceCollection services)
        {
            var config = new RabbitMqConfig();
            Configuration.GetSection("RabbitMq").Bind(config);
            var rabbitMqFactory = RabbitMqFactory.Create(config);

            AddAwaiter(services, rabbitMqFactory);
            AddPublishing(services, rabbitMqFactory);
        }

        private void AddAwaiter(IServiceCollection services, RabbitMqFactory factory)
        {
            var awaiter = new EventsAwaiter("APIGateway");

            var consumer = factory.CreateConsumer(Queues.APIGateway);
            awaiter.BindConsumer(consumer);

            services.AddSingleton(awaiter);
        }

        private void AddPublishing(IServiceCollection services, RabbitMqFactory factory)
        {
            //TODO przepisac na builder
            var publishers = new Dictionary<string, IPublisher>();
            publishers.Add(Queues.Accounts, factory.CreatePublisher(Queues.Accounts));
            publishers.Add(Queues.Transactions, factory.CreatePublisher(Queues.Transactions));
            publishers.Add(Queues.Cards, factory.CreatePublisher(Queues.Cards));
            publishers.Add(Queues.Loans, factory.CreatePublisher(Queues.Loans));
            publishers.Add(Queues.Payments, factory.CreatePublisher(Queues.Payments));
            publishers.Add(Queues.Users, factory.CreatePublisher(Queues.Users));
            services.AddSingleton(new PublishingRouter(publishers));
        }
    }
}

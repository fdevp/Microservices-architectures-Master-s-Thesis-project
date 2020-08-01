using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Requester.RunningModes;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace Requester
{
    public static class ConfigureServices
    {
        public static void Configure(IServiceCollection serviceCollection)
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("log.txt")
                .CreateLogger();
            serviceCollection.AddSingleton<ILogger>(logger);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            var settings = new Settings();
            configuration.GetSection("Settings").Bind(settings);
            serviceCollection.AddSingleton(configuration);
            serviceCollection.AddSingleton(settings);


            var httpClient = new HttpClient { BaseAddress = new Uri(settings.Address)};
            serviceCollection.AddSingleton(httpClient);

            serviceCollection.AddSingleton<SetupMode>();
            serviceCollection.AddSingleton<BusinessClientMode>();
            serviceCollection.AddSingleton<IndividualClientMode>();
            serviceCollection.AddSingleton<ReportsMode>();
            serviceCollection.AddSingleton<AutomatMode>();
            serviceCollection.AddTransient<Runner>();

        }
    }
}

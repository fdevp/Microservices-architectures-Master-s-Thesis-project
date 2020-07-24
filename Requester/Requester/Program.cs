using CommandLine;
using Jil;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Requester.RunningModes;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Requester
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices.Configure(serviceCollection);
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            await serviceProvider.GetService<Runner>().Run();
        }
    }
}

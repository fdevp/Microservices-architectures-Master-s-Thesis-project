using Microsoft.Extensions.Configuration;
using Requester.RunningModes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Requester
{
    public class Runner
    {
        private readonly IConfigurationRoot configuration;

        public Runner(IConfigurationRoot configuration)
        {
            this.configuration = configuration;
        }

        public async Task Run()
        {
            var settings = new Settings();
            configuration.GetSection("Settings").Bind(settings);

            switch (settings.Mode)
            {
                case "setup":
                    new SetupMode(,);
                    break;
                default:
                    throw InvalidOperationException("Unknown working mode.");
            }
        }
    }
}

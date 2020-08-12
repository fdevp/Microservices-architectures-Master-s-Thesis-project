using Microsoft.Extensions.Configuration;
using Requester.Data;
using Requester.RunningModes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Requester
{
    public class Runner
    {
        private Settings settings;
        public SetupMode setupMode;
        private readonly BusinessClientMode businessClientMode;
        private readonly IndividualClientMode individualClientMode;
        private readonly UserActivityReportsMode userActivityReportsMode;
        private readonly OverallReportsMode overallReportsMode;
        private readonly AutomatMode automatMode;

        public Runner(Settings settings,
            SetupMode setupMode,
            BusinessClientMode businessClientMode,
            IndividualClientMode individualClientMode,
            UserActivityReportsMode userActivityReportsMode,
            OverallReportsMode overallReportsMode,
            AutomatMode automatMode)
        {
            this.settings = settings;
            this.setupMode = setupMode;
            this.businessClientMode = businessClientMode;
            this.individualClientMode = individualClientMode;
            this.userActivityReportsMode = userActivityReportsMode;
            this.overallReportsMode = overallReportsMode;
            this.automatMode = automatMode;
        }
        
        public async Task Run()
        {
            switch (settings.Mode)
            {
                case "setup":
                    setupMode.Perform();
                    break;
                case "individual":
                    individualClientMode.Perform();
                    break;
                case "business":
                    businessClientMode.Perform();
                    break;
                case "userActivityReport":
                    userActivityReportsMode.Perform();
                    break;
                case "overallReport":
                    overallReportsMode.Perform();
                    break;
                case "automat":
                    automatMode.Perform();
                    break;
                default:
                    throw new InvalidOperationException("Unknown working mode.");
            }
        }
    }
}

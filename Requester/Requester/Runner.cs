﻿using Microsoft.Extensions.Configuration;
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
        private readonly ReportsMode reportsMode;
        private readonly AutomatMode automatMode;

        public Runner(Settings settings,
            SetupMode setupMode,
            BusinessClientMode businessClientMode,
            IndividualClientMode individualClientMode,
            ReportsMode reportsMode,
            AutomatMode automatMode)
        {
            this.settings = settings;
            this.setupMode = setupMode;
            this.businessClientMode = businessClientMode;
            this.individualClientMode = individualClientMode;
            this.reportsMode = reportsMode;
            this.automatMode = automatMode;
        }
        
        public async Task Run()
        {
            Task action;
            switch (settings.Mode)
            {
                case "setup":
                    await setupMode.Perform();
                    break;
                case "individual":
                    await individualClientMode.Perform();
                    break;
                case "business":
                    await businessClientMode.Perform();
                    break;
                case "report":
                    await reportsMode.Perform();
                    break;
                case "automat":
                    await automatMode.Perform();
                    break;
                default:
                    throw new InvalidOperationException("Unknown working mode.");
            }
        }
    }
}

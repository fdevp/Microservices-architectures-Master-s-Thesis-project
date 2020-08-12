using Requester.Data;
using Requester.Requests;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Requester
{
    public class OverallReportsMode
    {
        private readonly HttpClient httpClient;
        private readonly Settings settings;
        private readonly SessionRequester sessionRequester;
        private readonly ILogger logger;

        public OverallReportsMode(HttpClient httpClient, Settings settings, SessionRequester sessionRequester, ILogger logger)
        {
            this.httpClient = httpClient;
            this.settings = settings;
            this.sessionRequester = sessionRequester;
            this.logger = logger;
        }

        public void Perform()
        {
            var scenarioFileContent = File.ReadAllText("overallReportScenario.json");
            var scenario = scenarioFileContent.Split('|');
            var options = new ParallelOptions { MaxDegreeOfParallelism = settings.Threads == 0 ? 1 : settings.Threads };

            var overallTimer = Stopwatch.StartNew();
            Parallel.ForEach(scenario, options, element =>
            {
                var scenarioTimer = Stopwatch.StartNew();
                var scenarioId = Guid.NewGuid().ToString();

                var scenarioPartTimer = Stopwatch.StartNew();
                var token = sessionRequester.GetToken("analyst", scenarioId);
                logger.Information($"Service='Requester' ScenarioId='{scenarioId}' Method='overallReport token' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");

                scenarioPartTimer.Restart();
                Report(element, scenarioId);
                logger.Information($"Service='Requester' ScenarioId='{scenarioId}' Method='overallReport report' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");

                scenarioPartTimer.Restart();
                sessionRequester.Logout(token, scenarioId);
                logger.Information($"Service='Requester' ScenarioId='{scenarioId}' Method='overallReport logout' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");

                logger.Information($"Service='Requester' ScenarioId='{scenarioId}' Method='overallReport scenario' Processing='{scenarioTimer.ElapsedMilliseconds}'");
            });

            logger.Information($"Service='Requester' Method='overallReport overall' Processing='{overallTimer.ElapsedMilliseconds}'");
        }

        private void Report(string element, string scenarioId)
        {
            var content = new StringContent(element, Encoding.UTF8, "application/json");
            content.Headers.Add("flowId", scenarioId);
            var result = httpClient.PostAsync("report/Overall", content).Result;
        }
    }
}

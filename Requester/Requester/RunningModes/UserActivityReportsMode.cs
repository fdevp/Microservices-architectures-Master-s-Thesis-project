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
    public class UserActivityReportsMode
    {
        private readonly HttpClient httpClient;
        private readonly Settings settings;
        private readonly SessionRequester sessionRequester;
        private readonly ILogger logger;

        public UserActivityReportsMode(HttpClient httpClient, Settings settings, SessionRequester sessionRequester, ILogger logger)
        {
            this.httpClient = httpClient;
            this.settings = settings;
            this.sessionRequester = sessionRequester;
            this.logger = logger;
        }

        public void Perform()
        {
            var scenarioFileContent = File.ReadAllText("userActivityReportScenario.json");
            var scenario = scenarioFileContent.Split('|');
            var options = new ParallelOptions { MaxDegreeOfParallelism = settings.Threads == 0 ? 1 : settings.Threads };
            Parallel.ForEach(scenario, options, element =>
            {
                var scenarioTimer = Stopwatch.StartNew();
                var scenarioId = Guid.NewGuid().ToString();

                var scenarioPartTimer = Stopwatch.StartNew();
                var token = sessionRequester.GetToken("analyst", scenarioId);
                logger.Information($"Service='Requester' ScenarioId='{scenarioId}' Method='userActivityReport token' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");
                
                scenarioPartTimer.Restart();
                Report(element, scenarioId);
                logger.Information($"Service='Requester' ScenarioId='{scenarioId}' Method='userActivityReport report' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");

                scenarioPartTimer.Restart();
                sessionRequester.Logout(token, scenarioId);
                logger.Information($"Service='Requester' ScenarioId='{scenarioId}' Method='userActivityReport logout' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");

                logger.Information($"Service='Requester' ScenarioId='{scenarioId}' Method='userActivityReport scenario' Processing='{scenarioTimer.ElapsedMilliseconds}'");
            });
        }

        private void Report(string element, string scenarioId)
        {
            var content = new StringContent(element, Encoding.UTF8, "application/json");
            content.Headers.Add("flowId", scenarioId);
            var result = httpClient.PostAsync("report/UserActivity", content).Result;
        }
    }
}

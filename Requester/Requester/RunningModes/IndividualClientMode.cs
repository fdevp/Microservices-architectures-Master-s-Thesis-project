using Newtonsoft.Json;
using Requester.Requests;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Requester.RunningModes
{
    public class IndividualClientMode
    {
        private readonly HttpClient httpClient;
        private readonly SessionRequester sessionRequester;
        private readonly ILogger logger;

        public IndividualClientMode(HttpClient httpClient, SessionRequester sessionRequester, ILogger logger)
        {
            this.httpClient = httpClient;
            this.sessionRequester = sessionRequester;
            this.logger = logger;
        }

        public void Perform()
        {
            var scenarioFileContent = File.ReadAllText("individual.json");
            var scenarios = JsonConvert.DeserializeObject<IndividualUserScenarioElement[]>(scenarioFileContent);
            var groups = scenarios.GroupBy(s => s.Group);
            var groupsCount = groups.Count();
            var options = new ParallelOptions { MaxDegreeOfParallelism = groupsCount };

            var overallTimer = Stopwatch.StartNew();
            Parallel.ForEach(groups, options, group =>
            {
                var index = 0;
                foreach (var element in group)
                {
                    var scenarioNo = $"{group.Key}_{index}";
                    var scenarioTimer = Stopwatch.StartNew();
                    var scenarioId = Guid.NewGuid().ToString();

                    var scenarioPartTimer = Stopwatch.StartNew();
                    var token = sessionRequester.GetToken(element.User, scenarioId);
                    logger.Information($"Service='Requester' ScenarioNo='{scenarioNo}' ScenarioId='{scenarioId}' Method='individual token' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");

                    scenarioPartTimer.Restart();
                    Balance(element.AccountId, scenarioId);
                    logger.Information($"Service='Requester' ScenarioNo='{scenarioNo}' ScenarioId='{scenarioId}' Method='individual balance' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");

                    scenarioPartTimer.Restart();
                    Transfer(element, scenarioId);
                    logger.Information($"Service='Requester' ScenarioNo='{scenarioNo}' ScenarioId='{scenarioId}' Method='individual transfer' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");

                    scenarioPartTimer.Restart();
                    sessionRequester.Logout(token, scenarioId);
                    logger.Information($"Service='Requester' ScenarioNo='{scenarioNo}' ScenarioId='{scenarioId}' Method='individual logout' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");

                    logger.Information($"Service='Requester' ScenarioNo='{scenarioNo}' ScenarioId='{scenarioId}' Method='individual scenario' Processing='{scenarioTimer.ElapsedMilliseconds}'");
                    index++;
                }
            });

            logger.Information($"Service='Requester' Method='individual overall' Processing='{overallTimer.ElapsedMilliseconds}'");
        }

        public void Balance(string accountId, string scenarioId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"account/balance/{accountId}");
            request.Headers.Add("flowId", scenarioId);
            var result = httpClient.SendAsync(request).Result;
        }

        public void Transfer(IndividualUserScenarioElement element, string scenarioId)
        {
            var body = JsonConvert.SerializeObject(element);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            content.Headers.Add("flowId", scenarioId);
            var result = httpClient.PostAsync("card/transfer", content).Result;
        }
        
    }
}

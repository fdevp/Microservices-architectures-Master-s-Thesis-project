using Jil;
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
            var scenarios = JSON.Deserialize<IndividualUserScenarioElement[]>(scenarioFileContent);
            var groups = scenarios.GroupBy(s => s.Group);
            var groupsCount = groups.Count();
            var options = new ParallelOptions { MaxDegreeOfParallelism = groupsCount };

            var overallTimer = Stopwatch.StartNew();
            Parallel.ForEach(groups, options, group =>
            {
                foreach (var element in group)
                {
                    var scenarioTimer = Stopwatch.StartNew();
                    var scenarioId = Guid.NewGuid().ToString();

                    var scenarioPartTimer = Stopwatch.StartNew();
                    var token = sessionRequester.GetToken(element.User);
                    logger.Information($"Service='Requester' ScenarioId='{scenarioId}' Method='individual token' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");
                    
                    //todo check account balance

                    scenarioPartTimer.Restart();
                    Transfer(element);
                    logger.Information($"Service='Requester' ScenarioId='{scenarioId}' Method='individual transfer' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");

                    scenarioPartTimer.Restart();
                    sessionRequester.Logout(token);
                    logger.Information($"Service='Requester' ScenarioId='{scenarioId}' Method='individual logout' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");

                    logger.Information($"Service='Requester' ScenarioId='{scenarioId}' Method='individual scenario' Processing='{scenarioTimer.ElapsedMilliseconds}'");
                }
            });

            logger.Information($"Service='Requester' Method='individual overall' Processing='{overallTimer.ElapsedMilliseconds}'");
        }

        public void Transfer(IndividualUserScenarioElement element)
        {
            var body = JSON.Serialize(element);
            var result = httpClient.PostAsync("card/transfer", new StringContent(body, Encoding.UTF8, "application/json")).Result;
        }
        
    }
}

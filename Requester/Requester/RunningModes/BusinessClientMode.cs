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
    public class BusinessClientMode
    {
        private readonly HttpClient httpClient;
        private readonly SessionRequester sessionRequester;
        private readonly ILogger logger;

        public BusinessClientMode(HttpClient httpClient, SessionRequester sessionRequester, ILogger logger)
        {
            this.httpClient = httpClient;
            this.sessionRequester = sessionRequester;
            this.logger = logger;
        }

        public void Perform()
        {
            var scenarioFileContent = File.ReadAllText("business.json");
            var scenarios = JsonConvert.DeserializeObject<BusinessUserScenarioElement[]>(scenarioFileContent);
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
                    var token = sessionRequester.GetToken(element.User, scenarioId);
                    logger.Information($"Service='Requester' ScenarioId='{scenarioId}' Method='business token' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");

                    //todo check account balance
                    scenarioPartTimer.Restart();
                    Panel(element.UserId, scenarioId);
                    logger.Information($"Service='Requester' ScenarioId='{scenarioId}' Method='business panel' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");

                    foreach (var transaction in element.Transactions)
                    {
                        scenarioPartTimer.Restart();
                        Transfer(transaction, scenarioId);
                        logger.Information($"Service='Requester' ScenarioId='{scenarioId}' Method='business transfer' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");
                    }
                    
                    scenarioPartTimer.Restart();
                    sessionRequester.Logout(token, scenarioId);
                    logger.Information($"Service='Requester' ScenarioId='{scenarioId}' Method='business logout' Processing='{scenarioPartTimer.ElapsedMilliseconds}'");

                    logger.Information($"Service='Requester' ScenarioId='{scenarioId}' Method='business scenario' Processing='{scenarioTimer.ElapsedMilliseconds}'");
                }
            });

            logger.Information($"Service='Requester' Method='business overall' Processing='{overallTimer.ElapsedMilliseconds}'");
        }

        public void Panel(string userId, string scenarioId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"user/{userId}/panel");
            request.Headers.Add("flowId", scenarioId);
            var result = httpClient.SendAsync(request).Result;
        }

        public void Transfer(BusinessUserTransaction element, string scenarioId)
        {
            var body = JsonConvert.SerializeObject(element);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            content.Headers.Add("flowId", scenarioId);
            var result = httpClient.PostAsync("account/transfer", content).Result;
        }
    }
}

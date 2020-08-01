using Jil;
using Serilog;
using System;
using System.Collections.Generic;
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
        private readonly ILogger logger;

        public IndividualClientMode(HttpClient httpClient, ILogger logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }

        public async Task Perform()
        {
            var scenarioFileContent = File.ReadAllText("individual.json");
            var scenarios = JSON.Deserialize<IndividualUserScenarioElement[]>(scenarioFileContent);
            var groups = scenarios.GroupBy(s => s.Group);
            var groupsCount = groups.Count();
            var options = new ParallelOptions { MaxDegreeOfParallelism = groupsCount };

            Parallel.ForEach(groups, options, async group =>
            {
                foreach (var element in group)
                {

                    var token = GetToken(element.User);
                    //todo check account balance
                    Transfer(element);
                    Logout(token);
                }
            });
        }

        public string GetToken(string username)
        {
            var body = JSON.Serialize(new TokenRequest { Login = username, Password = "password" });
            var result = httpClient.PostAsync("user/token", new StringContent(body, Encoding.UTF8, "application/json")).Result;
            return result.Content.ReadAsStringAsync().Result;
        }

        public void Transfer(IndividualUserScenarioElement element)
        {
            var body = JSON.Serialize(element);
            var result = httpClient.PostAsync("card/transfer", new StringContent(body, Encoding.UTF8, "application/json")).Result;
        }

        public void Logout(string token)
        {
            var body = JSON.Serialize(new LogoutRequest { Token = token });
            var result = httpClient.PostAsync("user/logout", new StringContent(body, Encoding.UTF8, "application/json")).Result;
        }
    }
}

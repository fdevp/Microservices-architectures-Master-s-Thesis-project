using System.Threading.Tasks;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PanelsBranchMicroservice;
using UsersMicroservice;
using static PanelsBranchMicroservice.PanelsBranch;
using static UsersMicroservice.Users;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> logger;
        private readonly UsersClient usersClient;
        private readonly PanelsBranchClient panelBranchClient;
        private readonly Mapper mapper;

        public UserController(ILogger<UserController> logger, UsersClient usersClient, PanelsBranchClient panelBranchClient, Mapper mapper)
        {
            this.logger = logger;
            this.usersClient = usersClient;
            this.panelBranchClient = panelBranchClient;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("{userId}/panel")]
        public async Task<Panel> Panel(string userId)
        {
            var flowId = (long)HttpContext.Items["flowId"];
            var response = await panelBranchClient.GetAsync(new GetPanelRequest { FlowId = flowId, UserId = userId });
            return mapper.Map<Panel>(response);
        }

        [HttpPost]
        [Route("token")]
        public async Task<string> Token(TokenRequest data)
        {
            var request = mapper.Map<SignInRequest>(data);
            request.FlowId = (long)HttpContext.Items["flowId"];
            var response = await usersClient.TokenAsync(request);
            return response.Token;
        }

        [HttpPost]
        [Route("logout")]
        public async Task Logout(Models.LogoutRequest data)
        {
            var request = new UsersMicroservice.LogoutRequest { Token = data.Token };
            request.FlowId = (long)HttpContext.Items["flowId"];
            await usersClient.LogoutAsync(request);
        }

        [HttpPost]
        [Route("setup")]
        public async Task Setup(UsersSetup setup)
        {
            var request = mapper.Map<SetupRequest>(setup);
            await usersClient.SetupAsync(request);
        }

        [HttpPost]
        [Route("teardown")]
        public async Task TearDown()
        {
            await usersClient.TearDownAsync(new Empty());
        }
    }
}
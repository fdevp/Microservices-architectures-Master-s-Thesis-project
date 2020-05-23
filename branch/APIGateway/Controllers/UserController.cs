using System.Threading.Tasks;
using APIGateway.Models;
using APIGateway.Models.Setup;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UsersMicroservice;
using static UsersMicroservice.Users;

namespace APIGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> logger;
        private readonly UsersClient usersClient;
        private readonly Mapper mapper;

        public UserController(ILogger<UserController> logger, UsersClient usersClient, Mapper mapper)
        {
            this.logger = logger;
            this.usersClient = usersClient;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("token")]
        public async Task<string> Token(TokenRequest data)
        {
            var request = mapper.Map<SignInRequest>(data);
            var response = await usersClient.TokenAsync(request);
            return response.Token;
        }

        [HttpPost]
        [Route("logout")]
        public async Task Logout(string token)
        {
            var request = new LogoutRequest { Token = token };
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
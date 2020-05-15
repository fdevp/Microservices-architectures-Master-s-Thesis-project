using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace UsersMicroservice
{
    public class UsersService : Users.UsersBase
    {
        private readonly ILogger<UsersService> _logger;
        public UsersService(ILogger<UsersService> logger)
        {
            _logger = logger;
        }

        public override Task<TokenReply> Token(SignInRequest request, ServerCallContext context)
        {
            return Task.FromResult(new TokenReply{
                Token = Guid.NewGuid().ToString()
            });
        }

        public override Task<Empty> Logout(LogoutRequest request, ServerCallContext context)
        {
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> AddMessage(AddMessageRequest request, ServerCallContext context)
        {
            return Task.FromResult(new Empty());
        }
    }
}

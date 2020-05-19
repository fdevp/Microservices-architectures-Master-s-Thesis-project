using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using UsersMicroservice.Repository;

namespace UsersMicroservice
{
    public class UsersService : Users.UsersBase
    {
        private readonly ILogger<UsersService> _logger;
        private UsersRepository _repository;

        public UsersService(ILogger<UsersService> logger)
        {
            _logger = logger;
            _repository = new UsersRepository();
        }

        public override async Task<Empty> Setup(SetupRequest request, ServerCallContext context)
        {
            var users = request.Users.Select(u => new Repository.User(u.Id, u.Login, u.Password));
            var inboxes = request.Messages.GroupBy(m => m.UserId)
                .Select(g => new Inbox(g.Key, g.Select(g => g.Content).ToArray()));

            _repository.Setup(users, inboxes);
            return new Empty();
        }

        public override async Task<Empty> TearDown(Empty request, ServerCallContext context)
        {
            _repository.TearDown();
            return new Empty();
        }

        public override async Task<TokenReply> Token(SignInRequest request, ServerCallContext context)
        {
            var token = _repository.CreateSession(request.Login, request.Password);
            return new TokenReply { Token = token };
        }

        public override async Task<Empty> Logout(LogoutRequest request, ServerCallContext context)
        {
            _repository.RemoveSession(request.Token);
            return new Empty();
        }

        public override async Task<Empty> BatchAddMessages(BatchAddMessagesRequest request, ServerCallContext context)
        {
            foreach (var message in request.Messages)
            {
                _repository.AddMessage(message.UserId, message.Content);
            }
            return new Empty();
        }
    }
}

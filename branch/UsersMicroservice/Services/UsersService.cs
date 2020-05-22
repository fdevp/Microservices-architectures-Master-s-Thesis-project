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
        private readonly ILogger<UsersService> logger;
        private UsersRepository repository;

        public UsersService(ILogger<UsersService> logger)
        {
            this.logger = logger;
            repository = new UsersRepository();
        }

        public override Task<TokenReply> Token(SignInRequest request, ServerCallContext context)
        {
            var token = repository.CreateSession(request.Login, request.Password);
            return Task.FromResult(new TokenReply { Token = token });
        }

        public override Task<Empty> Logout(LogoutRequest request, ServerCallContext context)
        {
            repository.RemoveSession(request.Token);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> BatchAddMessages(BatchAddMessagesRequest request, ServerCallContext context)
        {
            foreach (var message in request.Messages)
            {
                repository.AddMessage(message.UserId, message.Content);
            }
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> Setup(SetupRequest request, ServerCallContext context)
        {
            var users = request.Users.Select(u => new Repository.User(u.Id, u.Login, u.Password));
            var inboxes = request.Messages.GroupBy(m => m.UserId)
                .Select(g => new Inbox(g.Key, g.Select(g => g.Content).ToArray()));

            repository.Setup(users, inboxes);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> TearDown(Empty request, ServerCallContext context)
        {
            repository.TearDown();
            return Task.FromResult(new Empty());
        }
    }
}

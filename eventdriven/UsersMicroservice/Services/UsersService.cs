
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharedClasses.Events.Users;
using SharedClasses.Messaging;
using SharedClasses.Models;
using UsersMicroservice.Repository;

namespace UsersMicroservice
{
    public class UsersService
    {
        private readonly ILogger<UsersService> logger;
        private readonly PublishingRouter publishingRouter;
        private UsersRepository usersRepository;

        public UsersService(UsersRepository usersRepository, ILogger<UsersService> logger, PublishingRouter publishingRouter)
        {
            this.usersRepository = usersRepository;
            this.logger = logger;
            this.publishingRouter = publishingRouter;
        }

        [EventHandlingMethod(typeof(CreateTokenEvent))]
        public Task Token(MessageContext context, CreateTokenEvent inputEvent)
        {
            var token = usersRepository.CreateSession(inputEvent.Login, inputEvent.Password);
            publishingRouter.Publish(context.ReplyTo, new NewTokenEvent { Token = token }, context.FlowId);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(LogoutEvent))]
        public Task Logout(MessageContext context, LogoutEvent inputEvent)
        {
            usersRepository.RemoveSession(inputEvent.Token);
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(BatchAddMessagesEvent))]
        public Task BatchAddMessages(MessageContext context, BatchAddMessagesEvent inputEvent)
        {
            foreach (var message in inputEvent.Messages)
            {
                usersRepository.AddMessage(message.UserId, message.Content);
            }
            return Task.CompletedTask;
        }

        [EventHandlingMethod(typeof(SetupUsersEvent))]
        public Task Setup(MessageContext context, SetupUsersEvent inputEvent)
        {
            var users = inputEvent.Users.Select(u => new User { Id = u.Id, Login = u.Login, Password = u.Password });
            var inboxes = inputEvent.Messages != null
                ? inputEvent.Messages.GroupBy(m => m.UserId).Select(g => new Inbox(g.Key, g.Select(g => g.Content).ToArray()))
                : new Inbox[0];

            usersRepository.Setup(users, inboxes);
            return Task.CompletedTask;
        }
    }
}

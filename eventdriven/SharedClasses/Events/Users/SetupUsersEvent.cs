using SharedClasses.Models;

namespace SharedClasses.Events.Users
{
    public class SetupUsersEvent
    {
        public User[] Users { get; set; }
        public UserMessage[] Messages { get; set; }
    }
}
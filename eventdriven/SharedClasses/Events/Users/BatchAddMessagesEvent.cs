using SharedClasses.Models;

namespace SharedClasses.Events.Users
{
    public class BatchAddMessagesEvent
    {
        public UserMessage[] Messages { get; set; }
    }
}
using System.Collections.Generic;
using System.Linq;

namespace UsersMicroservice.Repository
{
    public class Inbox
    {
        public string UserId { get; }

        public IReadOnlyList<string> Messages => _messages;

        private List<string> _messages { get; }

        public Inbox(string userId, params string[] messages)
        {
            UserId = userId;
            _messages = messages.ToList();
        }

        public void AddMessage(string message)
        {
            _messages.Add(message);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

namespace UsersMicroservice.Repository
{
	public class UsersRepository
    {
        private ConcurrentDictionary<string, Session> sessions = new ConcurrentDictionary<string, Session>();
        private ConcurrentDictionary<string, User> users = new ConcurrentDictionary<string, User>();
        private ConcurrentDictionary<string, Inbox> inboxes = new ConcurrentDictionary<string, Inbox>();


        public string CreateSession(string login, string password)
        {
            if (!users.ContainsKey(login))
                throw new ArgumentException("Login not found");

            var user = users[login];
            if (user.Password != password)
                throw new ArgumentException("Password do not match.");

            string token = Guid.NewGuid().ToString();
            sessions.TryAdd(token, new Session(user.Id, token));
            return token;
        }

        public void RemoveSession(string token)
        {
            sessions.TryRemove(token, out var removed);
        }

        public void AddMessage(string userId, string message)
        {
            if (inboxes.ContainsKey(userId))
                inboxes[userId].AddMessage(message);
            else
                inboxes.TryAdd(userId, new Inbox(userId, message));
        }

        public void Setup(IEnumerable<User> users, IEnumerable<Inbox> inboxes)
        {
            sessions = new ConcurrentDictionary<string, Session>();
            this.users = new ConcurrentDictionary<string, User>(users.ToDictionary(u => u.Login, u => u));
            this.inboxes = new ConcurrentDictionary<string, Inbox>(inboxes.ToDictionary(i => i.UserId, i => i));
        }
    }
}
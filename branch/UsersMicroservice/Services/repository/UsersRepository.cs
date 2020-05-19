

using System;
using System.Collections.Generic;
using System.Linq;

namespace UsersMicroservice.Repository
{
    public class UsersRepository
    {
        private Dictionary<string, Session> sessions = new Dictionary<string, Session>();
        private Dictionary<string, User> users = new Dictionary<string, User>();
        private Dictionary<string, Inbox> inboxes = new Dictionary<string, Inbox>();


        public string CreateSession(string login, string password)
        {
            if (!users.ContainsKey(login))
                throw new ArgumentException("Login not found");

            var user = users[login];
            if (user.Password != password)
                throw new ArgumentException("Password do not match.");

            string token = Guid.NewGuid().ToString();
            sessions.Add(token, new Session(user.Id, token));
            return token;
        }

        public void RemoveSession(string token)
        {
            sessions.Remove(token);
        }

        public void AddMessage(string userId, string message)
        {
            if (inboxes.ContainsKey(userId))
                inboxes[userId].AddMessage(message);
            else
                inboxes.Add(userId, new Inbox(userId, message));
        }

        public void Setup(IEnumerable<User> users, IEnumerable<Inbox> inboxes)
        {
            sessions = new Dictionary<string, Session>();
            this.users = users.ToDictionary(u => u.Login, u => u);
            this.inboxes = inboxes.ToDictionary(i => i.UserId, i => i);
        }

        public void TearDown()
        {
            sessions = new Dictionary<string, Session>();
            users = new Dictionary<string, User>();
            inboxes = new Dictionary<string, Inbox>();
        }
    }
}
using Jil;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Requester.Requests
{
    public class SessionRequester
    {
        private readonly HttpClient httpClient;

        public SessionRequester(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public string GetToken(string username)
        {
            var body = JSON.Serialize(new TokenRequest { Login = username, Password = "password" });
            var result = httpClient.PostAsync("user/token", new StringContent(body, Encoding.UTF8, "application/json")).Result;
            return result.Content.ReadAsStringAsync().Result;
        }

        public void Logout(string token)
        {
            var body = JSON.Serialize(new LogoutRequest { Token = token });
            var result = httpClient.PostAsync("user/logout", new StringContent(body, Encoding.UTF8, "application/json")).Result;
        }

        private class LogoutRequest
        {
            public string Token { get; set; }
        }

        private class TokenRequest
        {
            public string Login { get; set; }
            public string Password { get; set; }
        }
    }
}

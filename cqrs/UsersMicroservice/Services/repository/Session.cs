namespace UsersMicroservice.Repository
{
    public class Session
    {
        public string UserId { get; }
        public string Token { get; }

        public Session(string userId, string token)
        {
            UserId = userId;
            Token = token;
        }
    }
}
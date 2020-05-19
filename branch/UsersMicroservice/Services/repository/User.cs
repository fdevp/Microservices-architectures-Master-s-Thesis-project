namespace UsersMicroservice.Repository
{
    public class User
    {
        public string Id { get; }
        public string Login { get; }
        public string Password { get; }

        public User(string id, string login, string password)
        {
            Id = id;
            Login = login;
            Password = password;
        }
    }
}
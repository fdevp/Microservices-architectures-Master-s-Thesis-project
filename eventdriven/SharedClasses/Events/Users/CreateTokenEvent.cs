namespace SharedClasses.Events.Users
{
    public class CreateTokenEvent
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
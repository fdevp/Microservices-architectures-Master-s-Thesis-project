using SharedClasses.Models;

namespace SharedClasses.Events.Accounts
{
    public class SetupAccountsEvent
    {
        public Account[] Accounts { get; set; }
    }
}
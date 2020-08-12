using SharedClasses.Models;

namespace SharedClasses.Events.Accounts
{
    public class SetupAppendAccountsEvent
    {
        public Account[] Accounts { get; set; }
    }
}
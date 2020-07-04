using SharedClasses.Models;

namespace SharedClasses.Events.Accounts
{
    public class SelectedAccountsEvent
    {
        public Account[] Accounts { get; set; }
    }
}
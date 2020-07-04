using SharedClasses.Models;

namespace SharedClasses.Events.Accounts
{
    public class SelectedBalancesEvent
    {
        public AccountBalance[] Balances { get; set; }
    }
}
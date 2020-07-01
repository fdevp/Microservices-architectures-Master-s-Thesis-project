using SharedClasses.Models;

namespace SharedClasses.Events.Transactions
{
    public class SelectedTransactionsEvent
    {
        public Transaction[] Transactions { get; set; }
    }
}
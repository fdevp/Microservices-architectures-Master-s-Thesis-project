using SharedClasses.Models;

namespace SharedClasses.Events.Transactions
{
    public class SetupTransactionsEvent
    {
        public Transaction[] Transactions { get; set; }
    }
}
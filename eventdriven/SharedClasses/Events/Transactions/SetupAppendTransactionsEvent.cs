using SharedClasses.Models;

namespace SharedClasses.Events.Transactions
{
    public class SetupAppendTransactionsEvent
    {
        public Transaction[] Transactions { get; set; }
    }
}
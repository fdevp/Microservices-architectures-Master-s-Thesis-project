using SharedClasses.Models;

namespace SharedClasses.Events.Accounts
{
    public class BatchTransferEvent
    {
        public AccountTransfer[] Transfers { get; set; }
    }
}
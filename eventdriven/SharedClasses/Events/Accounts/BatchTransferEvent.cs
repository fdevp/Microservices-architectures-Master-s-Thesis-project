using SharedClasses.Models;

namespace SharedClasses.Events.Accounts
{
    public class BatchTransferEvent
    {
        public Transfer[] Transfers { get; set; }
    }
}
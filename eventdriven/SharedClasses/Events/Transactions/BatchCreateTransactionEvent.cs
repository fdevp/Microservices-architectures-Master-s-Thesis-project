namespace SharedClasses.Events.Transactions
{
    public class BatchCreateTransactionEvent
    {
        public CreateTransactionEvent[] Requests {get;set;}
    }
}
namespace SharedClasses.Events.Payments
{
    public class GetPaymentsByAccountsEvent
    {
        public string[] AccountsIds { get; set; }
    }
}
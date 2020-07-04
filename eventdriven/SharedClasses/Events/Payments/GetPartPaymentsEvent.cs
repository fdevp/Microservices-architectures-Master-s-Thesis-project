namespace SharedClasses.Events.Payments
{
    public class GetPartPaymentsEvent
    {
        public int Part { get; set; }
        public int TotalParts { get; set; }
    }
}
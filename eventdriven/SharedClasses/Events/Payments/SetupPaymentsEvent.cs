using SharedClasses.Models;

namespace SharedClasses.Events.Payments
{
    public class SetupPaymentsEvent
    {
        public Payment[] Payments { get; set; }
    }
}
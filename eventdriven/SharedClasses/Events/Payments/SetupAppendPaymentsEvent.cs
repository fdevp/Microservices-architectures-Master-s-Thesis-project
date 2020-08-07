using SharedClasses.Models;

namespace SharedClasses.Events.Payments
{
    public class SetupAppendPaymentsEvent
    {
        public Payment[] Payments { get; set; }
    }
}
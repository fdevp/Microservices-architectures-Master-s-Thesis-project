using SharedClasses.Models;

namespace SharedClasses.Events.Payments
{
    public class SelectedPaymentsEvent
    {
        public Payment[] Payments { get; set; }
    }
}
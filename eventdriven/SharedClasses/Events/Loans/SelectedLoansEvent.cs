using SharedClasses.Models;

namespace SharedClasses.Events.Loans
{
    public class SelectedLoansEvent
    {
        public Loan[] Loans { get; set; }
    }
}
using SharedClasses.Models;

namespace SharedClasses.Events.Loans
{
    public class SetupLoansEvent
    {
        public Loan[] Loans { get; set; }
    }
}
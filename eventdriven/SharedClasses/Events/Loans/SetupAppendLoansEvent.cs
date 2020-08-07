using SharedClasses.Models;

namespace SharedClasses.Events.Loans
{
    public class SetupAppendLoansEvent
    {
        public Loan[] Loans { get; set; }
    }
}
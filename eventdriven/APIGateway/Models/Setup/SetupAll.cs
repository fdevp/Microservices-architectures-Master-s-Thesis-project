namespace APIGateway.Models.Setup
{
    public class SetupAll
    {
        public AccountsSetup AccountsSetup { get; set; }
        public CardsSetup CardsSetup { get; set; }
        public LoansSetup LoansSetup { get; set; }
        public PaymentsSetup PaymentsSetup { get; set; }
        public TransactionsSetup TransactionsSetup { get; set; }
        public UsersSetup UsersSetup { get; set; }
    }
}
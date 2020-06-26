namespace APIGateway.Models
{
    public class Panel
    {
        public AccountDTO[] Accounts { get; set; }
        public CardDTO[] Cards { get; set; }
        public LoanDTO[] Loans { get; set; }
        public PaymentDTO[] Payments { get; set; }
        public TransactionDTO[] Transactions { get; set; }
    }
}
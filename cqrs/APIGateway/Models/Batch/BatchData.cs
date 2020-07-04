namespace APIGateway.Models
{
    public class BatchData
    {
        public LoanDTO[] Loans { get; set; }
        public PaymentDTO[] Payments { get; set; }
        public BalanceDTO[] Balances { get; set; }
    }
}
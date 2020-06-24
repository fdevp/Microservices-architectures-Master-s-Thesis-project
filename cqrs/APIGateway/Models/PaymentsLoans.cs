namespace APIGateway.Models
{
    public class PaymentsLoans
    {
        public LoanDTO[] Loans { get; set; }
        public PaymentDTO[] Payments { get; set; }
    }
}
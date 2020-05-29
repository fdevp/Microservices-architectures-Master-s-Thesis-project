namespace APIGateway.Models
{
    public class LoanDTO
    {
        public string Id { get; set; }
        public float PaidAmount { get; set; }
        public float TotalAmount { get; set; }
        public int Instalments { get; set; }
        public string PaymentId { get; set; }
    }
}
namespace LoansReadMicroservice.Repository
{
    public class Loan
    {
        public string Id { get; set; }
        public float PaidAmount { get; set; }
        public float TotalAmount { get; set; }
        public int Instalments { get; set; }
        public string PaymentId { get; set; }
        public string AccountId { get; set; }
    }
}
namespace LoansMicroservice.Repository
{
    public class Loan
    {
        public string Id { get; }
        public float PaidAmount { get; }
        float TotalAmount { get; }
        int Instalments { get; }

        string PaymentId { get; }

        public Loan(string id, float paidAmount, float totalAmount, int instalments, string paymentId)
        {
            Id = id;
            PaidAmount = paidAmount;
            TotalAmount = totalAmount;
            Instalments = instalments;
            PaymentId = paymentId;
        }
    }
}
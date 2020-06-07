namespace LoansReadMicroservice.Repository
{
    public class Loan
    {
        public string Id { get; }
        public float PaidAmount { get; private set; }
        public float TotalAmount { get; }
        public int Instalments { get; }

        public string PaymentId { get; }

        public Loan(string id, float paidAmount, float totalAmount, int instalments, string paymentId)
        {
            Id = id;
            PaidAmount = paidAmount;
            TotalAmount = totalAmount;
            Instalments = instalments;
            PaymentId = paymentId;
        }

        public void Repay(float amount)
        {
            this.PaidAmount += amount;
        }
    }
}
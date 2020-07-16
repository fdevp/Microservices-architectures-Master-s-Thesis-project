namespace LoansMicroservice.Repository
{
    public class Loan
    {
        public string Id { get; }
        public float PaidAmount { get; private set; }
        public float TotalAmount { get; }
        public int Instalments { get; }
        public string PaymentId { get; }
        public string AccountId {get;}

        public Loan(string id, float paidAmount, float totalAmount, int instalments, string paymentId, string accountId)
        {
            Id = id;
            PaidAmount = paidAmount;
            TotalAmount = totalAmount;
            Instalments = instalments;
            PaymentId = paymentId;
            AccountId = accountId;
        }

        public void Repay(float amount)
        {
            this.PaidAmount += amount;
        }
    }
}
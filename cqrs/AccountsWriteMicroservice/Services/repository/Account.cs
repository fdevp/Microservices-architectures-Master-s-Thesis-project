namespace AccountsWriteMicroservice.Repository
{
    public class Account
    {
        public string Id { get; }
        public string UserId { get; }
        public float Balance { get; private set; }
        public string Number { get; }

        public Account(string id, string userId, float balance, string number)
        {
            this.Id = id;
            this.UserId = userId;
            this.Balance = balance;
            this.Number = number;
        }

        public void SetBalance(float amount)
        {
            Balance = amount;
        }
    }
}

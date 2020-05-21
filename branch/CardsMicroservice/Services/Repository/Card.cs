namespace CardsMicroservice.Repository
{
    public class Card
    {
        public string Id { get; }
        public string Number { get; }
        public string AccountId { get;}
        
        public Card(string id, string number, string accountId)
        {
            Id = id;
            Number = number;
            AccountId = accountId;
        }
    }
}
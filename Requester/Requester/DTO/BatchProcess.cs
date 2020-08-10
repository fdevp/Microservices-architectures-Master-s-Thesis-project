namespace Requester
{
    public class BatchProcess
    {
        public AccountTransfer[] Transfers { get; set; }
        public MessageDTO[] Messages { get; set; }
        public string[] RepaidInstalmentsIds { get; set; }
    }
}
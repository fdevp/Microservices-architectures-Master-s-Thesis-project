namespace SharedClasses.Messaging
{
    public class MessageContext
    {
        public string FlowId { get; set; }
        public string Type { get; set; }
        public string ReplyTo { get; set; }
    }
}
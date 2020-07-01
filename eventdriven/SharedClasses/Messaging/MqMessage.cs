namespace SharedClasses.Messaging
{
    public class MqMessage
    {
        public string Data { get; set; }
        public string FlowId { get; set; }
        public string Type { get; set; }
        public string ReplyTo { get; set; }

        public MqMessage(string data, string flowId, string type, string replyTo)
        {
            Data = data;
            FlowId = flowId;
            Type = type;
            ReplyTo = replyTo;
        }
    }
}
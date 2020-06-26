namespace SharedClasses.Messaging
{
    public interface IPublisher
    {
        void Publish(object message, string flowId);
    }
}
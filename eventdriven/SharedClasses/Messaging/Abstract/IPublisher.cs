namespace SharedClasses.Messaging
{
    public interface IPublisher
    {
        void Publish(string type, object message, string flowId, string nextRoute = null);
    }
}
using System;

namespace SharedClasses.Messaging
{
    public interface IConsumer
    {
        event EventHandler<MqMessage> Received;
    }
}
using System;
using Jil;

namespace SharedClasses.Messaging
{
    public static class MessageParser
    {
        public static object Parse(string type, string message)
        {
            switch (type)
            {
                case nameof(TestEvent):
                    return JSON.Deserialize<TestEvent>(message);
                default:
                    throw new InvalidOperationException("Unknown event type.");
            }
        }
    }
}
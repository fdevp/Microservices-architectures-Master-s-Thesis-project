using System;

namespace SharedClasses.Messaging
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EventHandler : Attribute
    {
        public string Type { get; }
        public EventHandler(string type)
        {
            Type = type;
        }
    }
}
using System;

namespace SharedClasses.Messaging
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EventHandlingMethod : Attribute
    {
        public string Type { get; }
        public EventHandlingMethod(string type)
        {
            Type = type;
        }

        public EventHandlingMethod(Type type) : this(type.ToString()) { }
    }
}
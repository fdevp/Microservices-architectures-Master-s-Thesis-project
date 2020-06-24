using System;

namespace SharedClasses.Commands
{
    public class Command
    {
        public DateTime Timestamp { get; }
        public Type Type { get; }
        public object Data { get; }
        public long FlowId { get; }

        public Command(object command, long flowId)
        {
            this.Timestamp = DateTime.UtcNow;
            this.Type = command.GetType();
            this.Data = command;
            this.FlowId = flowId;
        }
    }
}
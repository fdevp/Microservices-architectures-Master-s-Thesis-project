using System.Collections.Generic;

namespace SharedClasses.Commands
{
    public class CommandsRepository
    {
        private readonly List<Command> commands;

        public Command[] Commands() => commands.ToArray();

        public CommandsRepository()
        {
            this.commands = new List<Command>();
        }

        public void Add(object command, long flowId)
        {
            this.commands.Add(new Command(command, flowId));
        }

    }
}
using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Andromeda.Events.EventArguments
{
    public class CommandRunArgs : EventArgs
    {
        public IClient Sender { get; private set; }

        public Command Command { get; private set; }

        public string Arguments { get; private set; }

        public bool Fail { get; private set; }

        public CommandRunArgs(IClient sender, Command command, bool fail, string arguments)
        {
            Sender = sender;
            Command = command;
            Fail = fail;
            Arguments = arguments;
        }

        public void Deconstruct(out IClient sender, out Command command, out bool fail, out string arguments)
        {
            sender = Sender;
            command = Command;
            fail = Fail;
            arguments = Arguments;
        }
    }
}

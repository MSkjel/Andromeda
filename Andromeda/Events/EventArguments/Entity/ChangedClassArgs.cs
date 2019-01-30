using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Andromeda.Events.EventArguments
{
    public class ChangedClassArgs : EventArgs
    {

        public Entity Player { get; private set; }

        public string ClassName { get; private set; }

        public ChangedClassArgs(Entity player, string className)
        {
            Player = player;
            ClassName = className;
        }

        public void Deconstruct(out Entity player, out string className)
        {
            player = Player;
            className = ClassName;
        }
    }
}

using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Andromeda.Events.EventArguments
{
    public class GrenadePullbackArgs : EventArgs
    {
        public Entity Player { get; private set; }

        public string Grenade { get; private set; }

        public GrenadePullbackArgs(Entity player, string grenade)
        {
            Player = player;
            Grenade = grenade;
        }

        public void Deconstruct(out Entity player, out string grenade)
        {
            player = Player;
            grenade = Grenade;
        }
    }
}

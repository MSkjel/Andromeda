using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Andromeda.Events.EventArguments
{
    public class GrenadeFireArgs : EventArgs
    {
        public Entity Player { get; private set; }
        public Entity GrenadeEnt { get; private set; }

        public string Grenade { get; private set; }

        public GrenadeFireArgs(Entity player, Entity grenadeEnt, string grenade)
        {
            Player = player;
            GrenadeEnt = grenadeEnt;
            Grenade = grenade;
        }

        public void Deconstruct(out Entity player, out Entity grenadeEnt, out string grenade)
        {
            player = Player;
            grenadeEnt = GrenadeEnt;
            grenade = Grenade;
        }
    }
}

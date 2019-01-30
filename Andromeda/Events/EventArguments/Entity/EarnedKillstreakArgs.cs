using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Andromeda.Events.EventArguments
{
    public class EarnedKillstreakArgs : EventArgs
    {
        public Entity Player { get; private set; }

        public string Killstreak { get; private set; }

        public EarnedKillstreakArgs(Entity player, string killstreak)
        {
            Player = player;
            Killstreak = killstreak;
        }

        public void Deconstruct(out Entity player, out string killstreak)
        {
            player = Player;
            killstreak = Killstreak;
        }
    }
}

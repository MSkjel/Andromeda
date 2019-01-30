using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Andromeda.Events.EventArguments
{
    public class ChangedTeamArgs : EventArgs
    {
        public Entity Player { get; private set; }

        public string FromTeam { get; private set; }
        public string ToTeam { get; private set; }

        public ChangedTeamArgs(Entity player, string fromTeam, string toTeam)
        {
            Player = player;
            FromTeam = fromTeam;
            ToTeam = toTeam;
        }

        public void Deconstruct(out Entity player, out string fromTeam, out string toTeam)
        {
            player = Player;
            fromTeam = FromTeam;
            toTeam = ToTeam;
        }
    }
}

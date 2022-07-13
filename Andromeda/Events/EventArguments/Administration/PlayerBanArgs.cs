using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Andromeda.Events.EventArguments.Administration
{
    public class PlayerBanArgs : EventArgs
    {
        public Entity Player { get; private set; }

        public string Issuer { get; private set; }

        public string Reason { get; private set; }

        public PlayerBanArgs(Entity player, string issuer, string reason)
        {
            Player = player;
            Issuer = issuer;
            Reason = reason;
        }

        public void Deconstruct(out Entity player, out string issuer, out string reason)
        {
            player = Player;
            issuer = Issuer;
            reason = Reason;
        }
    }
}

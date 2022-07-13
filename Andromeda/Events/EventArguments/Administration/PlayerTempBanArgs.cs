using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Andromeda.Events.EventArguments.Administration
{
    public class PlayerTempBanArgs : EventArgs
    {
        public Entity Player { get; private set; }

        public string Issuer { get; private set; }

        public string Reason { get; private set; }

        public TimeSpan TimeSpan { get; private set; }

        public PlayerTempBanArgs(Entity player, string issuer, string reason, TimeSpan timeSpan)
        {
            Player = player;
            Issuer = issuer;
            Reason = reason;
            TimeSpan = timeSpan;
        }

        public void Deconstruct(out Entity player, out string issuer, out string reason, out TimeSpan timeSpan)
        {
            player = Player;
            issuer = Issuer;
            reason = Reason;
            timeSpan = TimeSpan;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;

namespace Andromeda.Interfaces
{
    public interface IUtils : IFunctionality
    {
        ColorScheme ColorScheme { get; }

        string ServerName { get; }

        void SayAllPlayers(IEnumerable<string> message);

        void SayToPlayer(Entity player, IEnumerable<string> messages);
    }
}

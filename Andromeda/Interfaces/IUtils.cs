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

        void SayAll(IEnumerable<string> message);

        void SayTo(Entity player, IEnumerable<string> messages, bool raw = false);
    }
}

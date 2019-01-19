using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;

namespace Andromeda.Interfaces
{
    public interface IUtils : IFunctionality
    {
        void SayAll(IEnumerable<Msg> message);
        void SayTo(Entity player, IEnumerable<Msg> messages);
    }
}

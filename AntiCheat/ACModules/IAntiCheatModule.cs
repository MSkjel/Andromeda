using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntiCheat.ACModules
{
    public interface IAntiCheatModule
    {
        string Name { get; }
        string Description { get; }

        bool Enabled { get; set; }

        void RegisterEvents();

        Action<Entity, string> TakeAction { get; set; }
    }
}

using AntiCheat.ACModules;
using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntiCheat
{
    [Plugin]
    public static class Main
    {
        [EntryPoint]
        private static void Init()
        {
            Aimbot Aimbot = new Aimbot();
            SilentAim silentAim = new SilentAim();
        }      
    }
}

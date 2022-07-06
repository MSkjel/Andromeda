using AntiCheat.ACModules;
using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Andromeda;
using Andromeda.Parse;

namespace AntiCheat
{
    [Plugin]
    public static class Main
    {

        public static List<IAntiCheatModule> AntiCheatModules = new List<IAntiCheatModule>();

        [EntryPoint]
        private static void Init()
        {
            AntiCheatModules.AddRange(new List<IAntiCheatModule>()
            {
                new Aimbot(),
                new SilentAim(),
                new NoRecoil(),
                new SpinBot(),
                new ForceClass(),
                new Wallhack()
            });

            foreach (IAntiCheatModule module in AntiCheatModules)
            {
                if (module.Enabled)
                {
                    module.RegisterEvents();
                    Log.Debug("Registered events for Anti-Cheat module: " + module.Name);
                }
            }
        }

        static Main()
        {
            Config.Load();

            Common.Register(AntiCheat.Instance);
        }
    }
}

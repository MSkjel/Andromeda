#define LowMemory
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

        public static List<IAntiCheatModule> AntiCheatModules = new List<IAntiCheatModule>()
        {
            new SilentAim(),
            new NoRecoil(),
            new SpinBot(),
            //new AntiEntity(),
            new ForceClass(),
#if !LowMemory
            new Aimbot(),      
            new Wallhack(),
            new AntiProxy()
#endif
        };

        [EntryPoint]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private static void Init()
        {
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
            Common.Register(AntiCheat.Instance);
        }

        [Cleanup]
        private static void Cleanup()
        {
            AntiCheatModules.Clear();
            Config.Instance = null;
            Config.YAMLSerializer = null;
        }
    }
}

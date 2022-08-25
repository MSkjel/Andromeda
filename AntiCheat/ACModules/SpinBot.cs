using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Andromeda;
using Andromeda.Events;
using InfinityScript;

namespace AntiCheat.ACModules
{
    internal class SpinBot : IAntiCheatModule
    {
        public string Name => "Anti-SpinBot";

        //public string Description => "Checks if a player has SpinBot";

        public bool Enabled
        {
            get;
            set;
        } = Config.Instance.AntiSpinBot.Enabled;


        public void RegisterEvents()
        {
            Events.WeaponFired.Add((sender, args) =>
            {
                Entity ent = sender as Entity;

                if (ent.RequestPermission("anticheat.immune.spinbot", out _))
                    return;

                if (ent.GetPlayerAngles().Z > Config.Instance.AntiSpinBot.MaxAngle || ent.GetPlayerAngles().Z < -Config.Instance.AntiSpinBot.MaxAngle)
                    Common.Admin.Ban(ent, "AntiCheat", "Spinbot detected");
                else if (ent.GetPlayerAngles().Z > (Config.Instance.AntiSpinBot.MaxAngle / 2) + 5 || ent.GetPlayerAngles().Z < -(Config.Instance.AntiSpinBot.MaxAngle / 2) + 5)
                    Utils.WarnAdminsWithPerm(ent, "anticheat.warn.spinbot", $"%eYou might want to take a look at %p{ent.Name}%e. Spinbot suspected");
            });
        }
    }
}

using Andromeda;
using Andromeda.Events;
using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntiCheat.ACModules
{
    internal class NoRecoil : IAntiCheatModule
    {
        public string Name => "Anti-NoRecoil";

        //public string Description => "Checks if a player is using NoRecoil";

        public bool Enabled
        {
            get;
            set;
        } = Config.Instance.AntiNoRecoil.Enabled;


        public void RegisterEvents()
        {
            Events.WeaponFired.Add((sender, args) =>
            {
                Entity ent = sender as Entity;
                int PingLimit = 150;

                if (ent.RequestPermission("anticheat.immune.norecoil", out _))
                    return;


                if (ent.GetPlayerAngles().Z == 0 && !ent.CurrentWeapon.Contains("ac130"))
                {
                    ent.IncrementField("NoRecoil", 1);

                    float multiplier = ent.RealPing() > PingLimit ? ent.RealPing() / (float)PingLimit : 1f;
                    int limit = (int)Math.Ceiling(Config.Instance.AntiNoRecoil.MaxActionLimit * multiplier);

                    if (ent.IsFieldEqual("NoRecoil", limit))
                        Common.Admin.Ban(ent, "AntiCheat", $"^1No-Recoil detected. Weapon: ^7{ent.CurrentWeapon}");
                    else if (ent.IsFieldEqual("NoRecoil", (limit / 2) + 1))
                        Utils.WarnAdminsWithPerm(ent, "anticheat.warn.norecoil", $"%eYou might want to take a look at %p{ent.Name}%e. No-Recoil suspected. Using weapon: %h1{ent.CurrentWeapon.Split('_')?[1] ?? ""}%e. At: %h1{ent.GetField("NoRecoil")}%e/%h1{limit}%e. MP: %h1{multiplier:0.00}");
                }
                else
                    ent.SetField("NoRecoil", 0);
            });
        }
    }
}

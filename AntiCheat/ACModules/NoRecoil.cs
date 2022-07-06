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

        public string Description => "Checks if a player is using NoRecoil";

        public bool Enabled
        {
            get;
            set;
        } = Config.Instance.AntiNoRecoil.Enabled;

        public Action<Entity, string> TakeAction
        {
            get;
            set;
        } = new Action<Entity, string>((ent, reason) =>
        {
            Common.Admin.Ban(ent, "AntiCheat", reason);
        });

        public void RegisterEvents()
        {
            Events.WeaponFired.Add((sender, args) =>
            {
                Entity ent = sender as Entity;

                if (ent.RequestPermission("anticheat.immune.norecoil", out _))
                    return;

                if (ent.GetPlayerAngles().Z == 0 && !ent.CurrentWeapon.Contains("ac130"))
                {
                    ent.IncrementField("NoRecoil", 1);

                    int limit = (int)Math.Ceiling(Config.Instance.AntiNoRecoil.MaxActionLimit * (ent.Ping > 150 ? ((float)ent.Ping) / 150 : 1f));

                    if (ent.IsFieldEqual("NoRecoil", limit))
                    {
                        TakeAction(ent, $"^1No-Recoil detected. Weapon: ^7{ent.CurrentWeapon}");
                    }
                    else if (ent.IsFieldEqual("NoRecoil", (limit / 2) + 1))
                        Utils.WarnAdminsWithPerm(ent, "anticheat.warn.norecoil", $"%eYou might want to take a look at %p{ent.Name}%e. No-Recoil suspected. Using weapon: %h1{ent.CurrentWeapon}");
                }
                else
                    ent.SetField("NoRecoil", 0);
            });
        }
    }
}

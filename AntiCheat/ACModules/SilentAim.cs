using Andromeda;
using Andromeda.Events;
using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntiCheat.ACModules
{
    internal class SilentAim : IAntiCheatModule
    {
        public string Name => "Anti-SilentAim";

        public string Description => "Checks if a player has SilentAim";

        public bool Enabled
        {
            get;
            set;
        } = Config.Instance.AntiSilentAim.Enabled;

        public Action<Entity, string> TakeAction
        {
            get;
            set;
        } = new Action<Entity, string>((ent, reason) =>
        {
            Common.Admin.TempBan(ent, "AntiCheat", reason);
        });

        public void RegisterEvents()
        {
            Script.PlayerDamage.Add((sender, args) =>
            {
                Entity ent = sender as Entity;

                if (ent.RequestPermission("anticheat.immune.silentaim", out _))
                    return;

                if (!(args.Mod.Contains("BULLET") || args.Mod.Contains("HEADSHOT")) || !ent.IsPlayer || args.Inflictor != ent)
                    return;

                Vector3 toHit = GSCFunctions.VectorToAngles(args.Player.GetTagOrigin("j_mainroot") - ent.GetTagOrigin("j_head"));
                double dist = ent.GetPlayerAngles().DistanceToAngle(toHit);

                if (dist > Config.Instance.AntiSilentAim.MaxOffsetAngle && ent.Origin.DistanceTo2D(args.Player.Origin) > 220)
                {
                    ent.IncrementField("AntiSilentAim", 1);

                    if (ent.IsFieldEqual("AntiSilentAim", Config.Instance.AntiSilentAim.MaxActionLimit))
                        TakeAction(ent, "Silent-Aim detected");
                    else if (ent.IsFieldEqual("AntiSilentAim", (Config.Instance.AntiSilentAim.MaxActionLimit / 2) + 1))
                        Utils.WarnAdminsWithPerm(ent, "anticheat.warn.silentaim", $"%eYou might want to take a look at %p{ent.Name}%e. Silent-Aim suspected");
                }
            });
        }
    }
}

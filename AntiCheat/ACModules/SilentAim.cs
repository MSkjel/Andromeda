using Andromeda;
using Andromeda.Events;
using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntiCheat.ACModules
{
    public class SilentAim
    {
        public SilentAim()
        {
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            Script.PlayerDamage.Add((sender, args) =>
            {
                Entity ent = sender as Entity;

                if (args.Mod != "MOD_BULLET" || !ent.IsPlayer)
                    return;

                Vector3 toHit = GSCFunctions.VectorToAngles(args.Player.GetTagOrigin("j_mainroot") - ent.GetTagOrigin("j_head"));
                double dist = ent.GetPlayerAngles().DistanceToAngle(toHit);

                if (dist > 20)
                    ent.Tell($"Hax m8??? Hit: {args.Hitloc}. Anglez: {dist}");
            });
        }
    }
}

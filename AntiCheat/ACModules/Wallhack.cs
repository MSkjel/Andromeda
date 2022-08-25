#define LowMemory

#if !LowMemory
using Andromeda;
using Andromeda.Events;
using InfinityScript;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AntiCheat.ACModules
{
    internal class Wallhack : IAntiCheatModule
    {
        public string Name => "Anti-Wallhack";

        public string Description => "Checks if a player is using Wallhack";

        public bool Enabled
        {
            get;
            set;
        } = Config.Instance.AntiWallhack.Enabled;

       public Lazy<Action<Entity, string>> TakeAction
        {
            get;
            set;
        } = new Lazy<Action<Entity, string>>(delegate
        {
            return new Action<Entity, string>((ent, reason) =>
            {
                Common.Admin.Ban(ent, "AntiCheat", reason);
            });
        });

        public void RegisterEvents()
        {
            Events.PlayerSpawned.Add((sender, ent) =>
            {
                //if (ent.RequestPermission("anticheat.immune.wallhack", out _))
                //    return;

                Stopwatch watch = new Stopwatch();
                watch.Start();

                if (!ent.HasField("antiWallhackTimer"))
                    ent.SetField("antiWallhackTimer", new Parameter(watch));
            });

            Script.PlayerDamage.Add((sender, args) =>
            {

                Entity attacker = args.Inflictor;
                Entity victim = args.Player;

                //if (attacker.RequestPermission("anticheat.immune.forceclass", out _))
                //    return;

                if (!attacker.IsPlayer)
                    return;

                if (attacker == victim)
                    return;

                if (args.Damage >= victim.Health)
                    attacker.IncrementField("antiWallhackTimes", 1);

                //attacker.Tell($"SightConeTrace: {GSCFunctions.SightConeTrace(attacker, victim.GetTagOrigin(args.Hitloc))}");
                //attacker.Tell($"HitLoc Vector: {victim.GetTagOrigin(args.Hitloc)}");
                //attacker.Tell($"Hit at {args.Hitloc}");

                //if(GSCFunctions.SightTracePassed(attacker.GetTagOrigin("j_head"), victim.GetTagOrigin("j_" + args.Hitloc), false))
                //{
                //    attacker.Tell($"Passed SightTrace from j_head to j_{args.Hitloc}");
                //}

                if (!GSCFunctions.SightTracePassed(attacker.GetTagOrigin("j_head"), victim.GetTagOrigin("j_head"), false, attacker))
                {
                    if (args.Damage >= victim.Health)
                    {
                        attacker.IncrementField("antiWallhackTimesInvisible", 1);

                        if (attacker.HasField("antiWallhackTimer"))
                        {
                            Stopwatch watch = attacker.GetField<Stopwatch>("antiWallhackTimer");

                            if ((watch.ElapsedMilliseconds / 1000) >= Config.Instance.AntiWallhack.HitResetTime)
                            {
                                if (attacker.GetFieldOrVal("antiWallhackTimes", 0) > Config.Instance.AntiWallhack.MinHitTimes)
                                {
                                    float percentage = attacker.GetField<float>("antiWallhackTimesInvisible") / attacker.GetField<float>("antiWallhackTimes");

                                    if (percentage > Config.Instance.AntiWallhack.MaxHitPercentage)
                                    {
                                        Utils.WarnAdminsWithPerm(victim, "anticheat.warn.wallhack", $"%eYou might want to take a look at %p{attacker.Name}%e. Wallhack suspected: %h1{percentage}");

                                        //OnlyForTesting :P
                                        attacker.SetField("antiWallhackTimes", 0);
                                        attacker.SetField("antiWallhackTimesInvisible", 0);
                                    }
                                    else
                                    {
                                        attacker.SetField("antiWallhackTimes", 0);
                                        attacker.SetField("antiWallhackTimesInvisible", 0);
                                    }
                                }

                                watch.Restart();
                            }
                        }
                    }
                }
            });
        }
    }
}
#endif
using Andromeda;
using Andromeda.Events;
using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AdvancedAdmin
{
    public static class Utils
    {
        internal static readonly Lazy<List<string>> Perks = new Lazy<List<string>>(delegate
        {
            List<string> perks = new List<string>()
            {
                "specialty_longersprint",
                "specialty_fastreload",
                "specialty_scavenger",
                "specialty_blindeye",
                "specialty_paint",
                "specialty_hardline",
                "specialty_coldblooded",
                "specialty_quickdraw",
                "_specialty_blastshield",
                "specialty_detectexplosive",
                "specialty_autospot",
                "specialty_bulletaccuracy",
                "specialty_quieter",
                "specialty_stalker",
                "specialty_stopping_power",
                "specialty_uav",
                "specialty_bulletpenetration",
                "specialty_marksman",
                "specialty_sharp_focus",
                "specialty_holdbreathwhileads",
                "specialty_longerrange",
                "specialty_fastermelee",
                "specialty_reducedsway",
                "specialty_lightweight"
            };

            perks.AddRange(perks.Select(x => GSCFunctions.TableLookup("mp/perktable.csv", 1, x, 8)).ToList());

            return perks;
        });


        internal static void InitializeFly(Entity entity)
        {
            entity.NotifyOnPlayerCommand("upPressed", "+gostand");
            entity.NotifyOnPlayerCommand("upReleased", "-gostand");

            entity.NotifyOnPlayerCommand("downPressed", "+breath_sprint");
            entity.NotifyOnPlayerCommand("downReleased", "-breath_sprint");

            entity.NotifyOnPlayerCommand("forwardPressed", "+forward");
            entity.NotifyOnPlayerCommand("forwardReleased", "-forward");

            entity.NotifyOnPlayerCommand("backwardPressed", "+back");
            entity.NotifyOnPlayerCommand("backwardReleased", "-back");

            entity.NotifyOnPlayerCommand("leftPressed", "+moveleft");
            entity.NotifyOnPlayerCommand("leftReleased", "-moveleft");

            entity.NotifyOnPlayerCommand("rightPressed", "+moveright");
            entity.NotifyOnPlayerCommand("rightReleased", "-moveright");

            entity.OnNotify("upPressed", (ent) => entity.SetField("Up", 1));
            entity.OnNotify("downPressed", (ent) => entity.SetField("Down", 1));

            entity.OnNotify("forwardPressed", (ent) => entity.SetField("Forward", 1));
            entity.OnNotify("backwardPressed", (ent) => entity.SetField("Backward", 1));
            entity.OnNotify("leftPressed", (ent) => entity.SetField("Left", 1));
            entity.OnNotify("rightPressed", (ent) => entity.SetField("Right", 1));

            entity.OnNotify("upReleased", (ent) => entity.SetField("Up", 0));
            entity.OnNotify("downReleased", (ent) => entity.SetField("Down", 0));

            entity.OnNotify("forwardReleased", (ent) => entity.SetField("Forward", 0));
            entity.OnNotify("backwardReleased", (ent) => entity.SetField("Backward", 0));
            entity.OnNotify("leftReleased", (ent) => entity.SetField("Left", 0));
            entity.OnNotify("rightReleased", (ent) => entity.SetField("Right", 0));

            entity.SetField("InitializedFly", 1);
        }

        internal static void DoFly(Entity entity)
        {
            BaseScript.OnInterval(0, delegate
            { 
                if (!entity.IsFieldTrue("EnableFly"))
                    return false;

                if (entity.IsOnGround())
                    return true;

                Vector3 Velocity = entity.GetVelocity();

                Velocity.Z = 23;

                if (entity.IsFieldTrue("Up"))
                    Velocity.Z += 300;

                if (entity.IsFieldTrue("Down"))
                    Velocity.Z -= 300;

                if (!entity.IsFieldTrue("Forward")
                    && !entity.IsFieldTrue("Backward")
                    && !entity.IsFieldTrue("Left")
                    && !entity.IsFieldTrue("Right"))
                {
                    if (Velocity.X > 0)
                        Velocity.X -= 1;
                    else if (Velocity.X < 0)
                        Velocity.X += 1;

                    if (Velocity.Y > 0)
                        Velocity.Y -= 1;
                    else
                        Velocity.Y += 1;
                }

                entity.SetVelocity(Velocity);
                return true;
            });
        }

        public static float Difference(float a1, float a2) => 
            180 - Math.Abs(Math.Abs(a1 - a2 + 360) - 180);


        public static double DistanceToAngle(this Vector3 angle1, Vector3 angle2) => 
            Math.Sqrt(Math.Pow(Difference(angle1.X, angle2.X), 2) + Math.Pow(Difference(angle1.Y, angle2.Y), 2));


        internal static void AkimboPrimary(Entity player)
        {
            player.DisableWeaponSwitch();
            player.DisableWeaponPickup();
            player.AllowAds(false);

            Marshal.WriteInt32((IntPtr)0x01AC23C1, (0x38A4 * player.EntRef), 1);

            player.SetWeaponAmmoClip(player.CurrentWeapon, int.MaxValue, "right");
            player.SetWeaponAmmoClip(player.CurrentWeapon, int.MaxValue, "left");
            player.SetWeaponAmmoStock(player.CurrentWeapon, int.MaxValue);
        }

        static bool DamageModHandlerRegistered = false;
        internal static void DamageMod()
        {
            if (!DamageModHandlerRegistered)
            {
                Script.PlayerDamage.Add((sender, args) =>
                {
                    if (args.Player.IsFieldTrue("EatBullets"))
                    {
                        if (args.Player.Health > 10)
                        {
                            int damage = args.Damage;
                            args.Damage = (int)(damage * 0.4);
                        }
                    }
                    else if (args.Player.IsFieldTrue("GodMode"))
                    {
                        args.Damage = 0;
                    }

                    if (args.Attacker.IsFieldTrue("DoubleDamage"))
                    {
                        int damage = args.Damage; 
                        args.Damage = (int)(damage * 1.5);
                    }
                    else if (args.Attacker.IsFieldTrue("NerfDamage") && args.Player.Health > 20 && args.Mod != "MOD_MELEE")
                    {
                        int damage = args.Damage;
                        args.Damage = (int)(damage * 0.5);
                    }
                });

                DamageModHandlerRegistered = true;
            }
        }

        //internal static void MissileStrike(Entity sender)
        //{
        //    Random rand = new Random(DateTime.Now.Millisecond);

        //    int Height = 10000;
        //    int number = BaseScript.Players.Count;

        //    for (int i = 0; i < number; i++)
        //    {
        //        if (i < number/* && BaseScript.Players[i] != sender*/)
        //        {
        //            if (!BaseScript.Players[i].IsAlive || (BaseScript.Players[i].SessionTeam == sender.SessionTeam && sender.SessionTeam != "none"))
        //                return;

        //            Vector3 dest0 = BaseScript.Players[i].Origin;
        //            Vector3 dest1 = BaseScript.Players[i].Origin;

        //            if (rand.Next(0, 1000) > 500)
        //            {
        //                dest0.Z += Height;
        //                dest0.X += rand.Next(0, 5000);
        //                dest0.Y += rand.Next(0, 5000);

        //                //dest1.Z -= Height;
        //                //dest1.X -= rand.Next(0, 1000);
        //                //dest1.Y -= rand.Next(0, 100);
        //            }
        //            else
        //            {
        //                dest0.Z += Height;
        //                dest0.X -= rand.Next(0, 5000);
        //                dest0.Y -= rand.Next(0, 5000);

        //                //dest1.Z -= Height;
        //                //dest1.X += rand.Next(0, 1000);
        //                //dest1.Y += rand.Next(0, 100);
        //            }

        //            GSCFunctions.MagicBullet("uav_strike_projectile_mp", dest0, dest1, sender);
        //        }
        //    }
        //}

        static bool SilentAimHandlerRegistered = false;
        internal static void SetupSilentAim()
        {
            const string aimFrom = "j_head";
            const string aimAt = "j_mainroot";

            if (!SilentAimHandlerRegistered)
            {
                Events.WeaponFired.Add((senderArgs, args) =>
                {
                    Entity sender = senderArgs as Entity;

                    if (sender.IsFieldTrue("EnableSilentAim"))
                        foreach (Entity ent in BaseScript.Players)
                        {
                            if (sender == ent || !ent.IsAlive || ent.SessionTeam == "spectator")
                                continue;

                            if (GSCFunctions.SightTracePassed(sender.GetTagOrigin(aimFrom), ent.GetTagOrigin(aimAt), false))
                            {
                                Vector3 angles = GSCFunctions.VectorToAngles(ent.GetTagOrigin(aimAt) - sender.GetTagOrigin(aimFrom));

                                if (sender.GetPlayerAngles().DistanceToAngle(angles) < 15)
                                {
                                    GSCFunctions.MagicBullet(args.Weapon, sender.GetTagOrigin("tag_weapon_right"), ent.GetTagOrigin(aimAt), sender);

                                    break;
                                }
                            }
                        }
                });

                SilentAimHandlerRegistered = true;
            }
        }

        //internal static void SetObjectiveText()
        //{
        //    if (!string.IsNullOrEmpty(Config.Instance.Messages.ObjectiveTextAllies) || !string.IsNullOrEmpty(Config.Instance.Messages.ObjectiveTextAxis) ||
        //        !string.IsNullOrEmpty(Config.Instance.Messages.ObjectiveTextNone) || !string.IsNullOrEmpty(Config.Instance.Messages.ObjectiveTextSpectators))
        //    {
        //        BaseScript.OnInterval(10, () =>
        //        {
        //            foreach (Entity ent in BaseScript.Players)
        //            {
        //                if (ent.SessionTeam == "allies")
        //                    ent.SetClientDvar("cg_objectiveText", Config.Instance.Messages.ObjectiveTextAllies);
        //                else if (ent.SessionTeam == "axis")
        //                    ent.SetClientDvar("cg_objectiveText", Config.Instance.Messages.ObjectiveTextAxis);
        //                else if (ent.SessionTeam == "none")
        //                    ent.SetClientDvar("cg_objectiveText", Config.Instance.Messages.ObjectiveTextNone);
        //                else if (ent.SessionTeam == "spectator")
        //                    ent.SetClientDvar("cg_objectiveText", Config.Instance.Messages.ObjectiveTextSpectators);
        //            }

        //            return true;
        //        });
        //    }
        //}

        internal static void DoAimbot(Entity sender, bool autoshoot = false, bool reverse = false)
        {
            const string aimFrom = "j_head";
            const string aimAt = "j_mainroot";

            BaseScript.OnInterval(0, () =>
            {
                if (!sender.IsAlive || sender.SessionTeam == "spectator" || sender.SessionState != "playing")
                    return true;

                if (!sender.IsFieldTrue("EnableAimbot"))
                    return false;

                Entity target = null;

                foreach (Entity ent in BaseScript.Players)
                {
                    if (!ent.IsAlive || ent == sender)
                        continue;

                    if (sender.SessionTeam == ent.SessionTeam && (sender.SessionTeam != "none" || ent.SessionTeam == "spectator"))
                        continue;

                    if (!GSCFunctions.SightTracePassed(sender.GetTagOrigin(aimFrom), ent.GetTagOrigin(aimAt), false))
                        continue;

                    if (target != null)
                    {
                        if (GSCFunctions.Closer(target.GetTagOrigin(aimAt), sender.GetTagOrigin(aimFrom), ent.GetTagOrigin(aimAt)))
                            target = ent;
                    }
                    else
                        target = ent;

                    if (target != null && target.IsAlive)
                    {
                        Vector3 aim = GSCFunctions.VectorToAngles(target.GetTagOrigin(aimAt) - sender.GetTagOrigin(aimFrom));
                        aim.Z = sender.GetPlayerAngles().Z;

                        if (reverse)
                            sender.SetPlayerAngles(aim + new Vector3(0, -180, 0));
                        else
                        {
                            if (autoshoot)
                            {
                                sender.SetPlayerAngles(aim);
                                GSCFunctions.MagicBullet(sender.GetCurrentWeapon(), sender.GetTagOrigin(aimFrom), target.GetTagOrigin(aimAt), sender);
                            }
                            else if (sender.AdsButtonPressed())
                                sender.SetPlayerAngles(aim);
                        }
                    }
                }

                return true;
            });
        }


        internal static void SetClanTag(int clientnum, string clantag)
        {
            if (!string.IsNullOrWhiteSpace(clantag) && !string.IsNullOrEmpty(clantag))
            {
                clantag += '\0';
                byte[] array = Encoding.ASCII.GetBytes(clantag);

                Marshal.Copy(array, 0, Memory.CalculateClantagAddress(clientnum), array.Length);
                Marshal.WriteByte(Memory.CalculateUseClanTagAddress(clientnum), 1);
            }
            else
                Marshal.WriteByte(Memory.CalculateUseClanTagAddress(clientnum), 0);
        }


        internal static void SetTitle(int clientnum, string title)
        {
            if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrEmpty(title))
            {
                title += '\0';
                byte[] array = Encoding.ASCII.GetBytes(title);

                Marshal.Copy(array, 0, Memory.CalculateTitleAddress(clientnum), array.Length);
                Marshal.WriteByte(Memory.CalculateUseCustomTitleAddress(clientnum), 1);
            }
            else
                Marshal.WriteByte(Memory.CalculateUseCustomTitleAddress(clientnum), 0);
        }


        internal static void SetName(Entity ent, string name)
        {
            ent.SetField("Force_Name", name);

            if (!ent.IsFieldTrue("Force_Name_Active"))
            {
                ent.SetField("Force_Name_Active", true);

                BaseScript.OnInterval(100, () =>
                {
                    
                    byte[] array = Encoding.ASCII.GetBytes(ent.GetField<string>("Force_Name"));
                    Marshal.Copy(array, 0, Memory.CalculateNameAddress(ent.EntRef), array.Length);
                    Marshal.WriteByte(Memory.CalculateNameAddress(ent.EntRef) + array.Length, 0);

                    return true;
                });

            }
        }

        //#region Chopper
        //private static Entity LB = null;
        //private static Entity turret = null;

        //public static void helicopter_turret(Entity player)
        //{
        //    player.SetField("HasHeli", 1);
        //    player.PlayLocalSound("mp_bonus_start");
        //    player.SetField("OldWep1", player.CurrentWeapon);
        //    player.SetField("OldWep2", player.GetCurrentOffhand());
        //    player.TakeAllWeapons();
        //    player.Notify("using_remote");
        //    BaseScript.AfterDelay(1500, () => helicopter(player));
        //}

        //private static void lbExplode(Entity LB, Entity player)
        //{
        //    player.GiveWeapon(player.GetField<string>("OldWep1"));
        //    player.GiveWeapon(player.GetField<string>("OldWep2"));
        //    BaseScript.AfterDelay(100, () => player.SwitchToWeaponImmediate(player.GetField<string>("OldWep1")));
        //    LB.StopLoopSound();
        //    LB.Vibrate(GSCFunctions.AnglesToRight(LB.Origin), 100, 4, 2);

        //    BaseScript.AfterDelay(1000, () =>
        //    {
        //        GSCFunctions.PlayFX(96, LB.Origin);
        //        LB.PlaySound("cobra_helicopter_");
        //        LB.Delete();
        //    });
        //}

        //private static void helicopter(Entity player)
        //{
        //    LB = GSCFunctions.SpawnHelicopter(player, player.Origin + new Vector3(0, 0, 3000), player.GetPlayerAngles(), "littlebird_mp", "vehicle_little_bird_armed");
        //    turret = GSCFunctions.SpawnTurret("misc_turret", player.Origin, "littlebird_guard_minigun_mp");

        //    turret.LinkTo(LB, "tag_minigun_attach_left", new Vector3(30, 30, 0), new Vector3(0, 0, 0));
        //    turret.SetModel("weapon_minigun");
        //    turret.MakeUnUsable();
        //    turret.MakeTurretSolid();
        //    turret.SetCanDamage(true);
        //    player.RemoteControlVehicle(LB);
        //    player.RemoteControlTurret(turret);
            
        //    BaseScript.AfterDelay(60000, () =>
        //    {
        //        if (player.GetField<int>("HasHeli") == 1)
        //            DestroyChopper(player);
        //    });

        //    BaseScript.OnInterval(100, () =>
        //    {
        //        if (player.GetField<int>("HasHeli") == 0)
        //        {
        //            DestroyChopper(player);

        //            return false;
        //        }

        //        return true;
        //    });
        //}

        //public static void DestroyChopper(Entity player)
        //{
        //    player.SetField("HasHeli", 0);
        //    player.IPrintLnBold("^1Helicopter  - Gone");
        //    player.SetField("restoreWeapon", player.CurrentWeapon);
        //    player.SetField("pos", player.Origin);
        //    player.RemoteControlTurretOff(turret);
        //    turret.Delete();
        //    lbExplode(LB, player);
        //}
        //#endregion
    }
}

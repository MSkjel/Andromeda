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

        [DllImport("TeknoMW3S.dll", EntryPoint = "GI_AddTestClient")]
        public static extern int AddTestClient();

        internal static List<string> Perks = new List<string>()
        {
            "specialty_longersprint",
            "specialty_fastreload",
            "specialty_scavenger",
            "specialty_blindeye",
            "specialty_paint",
            "specialty_hardline",
            "specialty_coldblooded",
            "specialty_quickdraw",
            "specialty_twoprimaries",
            "specialty_assists",
            "specialty_blastshield",
            "specialty_detectexplosive",
            "specialty_autospot",
            "specialty_bulletaccuracy",
            "specialty_quieter",
            "specialty_stalker",
            "specialty_juiced",
            "specialty_finalstand",
            "specialty_revenge",
            "specialty_stopping_power",
            "specialty_uav",
            "specialty_bulletpenetration",
            "specialty_marksman",
            "specialty_bling",
            "specialty_sharp_focus",
            "specialty_armorpiercing",
            "specialty_holdbreathwhileads",
            "specialty_longerrange",
            "specialty_fastermelee",
            "specialty_reducedsway",
            "specialty_lightweight",
            "specialty_moredamage",
            "all_perks_bonus"
        };

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

        public static float Difference(float a1, float a2)
        {
            return 180 - Math.Abs(Math.Abs(a1 - a2 + 360) - 180);
        }

        public static double DistanceToAngle(this Vector3 angle1, Vector3 angle2)
        {
            return Math.Sqrt(Math.Pow(Difference(angle1.X, angle2.X), 2) + Math.Pow(Difference(angle1.Y, angle2.Y), 2));
        }

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

        internal static void UAV(Entity player)
        {
            Marshal.WriteInt32((IntPtr)0x01AC5571, (0x38A4 * player.EntRef), 1);
        }

        internal static void MissileStrike(Entity sender)
        {
            Random rand = new Random(DateTime.Now.Millisecond);

            int Height = 10000;
            int number = BaseScript.Players.Count;

            for (int i = 0; i < number; i++)
            {
                if (i < number/* && BaseScript.Players[i] != sender*/)
                {
                    if (!BaseScript.Players[i].IsAlive || (BaseScript.Players[i].SessionTeam == sender.SessionTeam && sender.SessionTeam != "none"))
                        return;

                    Vector3 dest0 = BaseScript.Players[i].Origin;
                    Vector3 dest1 = BaseScript.Players[i].Origin;

                    if (rand.Next(0, 1000) > 500)
                    {
                        dest0.Z += Height;
                        dest0.X += rand.Next(0, 5000);
                        dest0.Y += rand.Next(0, 5000);

                        //dest1.Z -= Height;
                        //dest1.X -= rand.Next(0, 1000);
                        //dest1.Y -= rand.Next(0, 100);
                    }
                    else
                    {
                        dest0.Z += Height;
                        dest0.X -= rand.Next(0, 5000);
                        dest0.Y -= rand.Next(0, 5000);

                        //dest1.Z -= Height;
                        //dest1.X += rand.Next(0, 1000);
                        //dest1.Y += rand.Next(0, 100);
                    }

                    GSCFunctions.MagicBullet("uav_strike_projectile_mp", dest0, dest1, sender);
                }
            }
        }

        internal static bool RegisteredSilentAim = false;
        internal static void SetupSilentAim()
        {
            const string aimFrom = "j_head";
            const string aimAt = "j_mainroot";

            if (!RegisteredSilentAim)
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
                                    GSCFunctions.MagicBullet(sender.GetCurrentWeapon(), sender.GetTagOrigin("tag_weapon_right"), ent.GetTagOrigin(aimAt), sender);

                                    break;
                                }
                            }
                        }
                });

                RegisteredSilentAim = true;
            }
        }

        internal static void SetObjectiveText()
        {
            if (!string.IsNullOrEmpty(Config.Instance.Messages.ObjectiveTextAllies) || !string.IsNullOrEmpty(Config.Instance.Messages.ObjectiveTextAxis) ||
                !string.IsNullOrEmpty(Config.Instance.Messages.ObjectiveTextNone) || !string.IsNullOrEmpty(Config.Instance.Messages.ObjectiveTextSpectators))
            {
                BaseScript.OnInterval(10, () =>
                {
                    foreach (Entity ent in BaseScript.Players)
                    {
                        if (ent.SessionTeam == "allies")
                            ent.SetClientDvar("cg_objectiveText", Config.Instance.Messages.ObjectiveTextAllies);
                        else if (ent.SessionTeam == "axis")
                            ent.SetClientDvar("cg_objectiveText", Config.Instance.Messages.ObjectiveTextAxis);
                        else if (ent.SessionTeam == "none")
                            ent.SetClientDvar("cg_objectiveText", Config.Instance.Messages.ObjectiveTextNone);
                        else if (ent.SessionTeam == "spectator")
                            ent.SetClientDvar("cg_objectiveText", Config.Instance.Messages.ObjectiveTextSpectators);
                    }

                    return true;
                });
            }
        }

        internal static void DoAimbot(Entity sender)
        {
            const string aimFrom = "j_head";
            const string aimAt = "j_mainroot";

            BaseScript.OnInterval(1, () =>
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

                        sender.SetPlayerAngles(aim);
                        GSCFunctions.MagicBullet(sender.GetCurrentWeapon(), sender.GetTagOrigin(aimFrom), target.GetTagOrigin(aimAt), sender);
                    }
                }

                return true;
            });
        }

        internal static void DoReverseAimbot(Entity sender)
        {
            const string aimFrom = "j_head";
            const string aimAt = "j_mainroot";

            BaseScript.OnInterval(0, () =>
            {
                if (!sender.IsAlive || sender.SessionTeam == "spectator" || sender.SessionState != "playing")
                    return true;

                if (!sender.IsFieldTrue("EnableReverseAimbot"))
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

                        sender.SetPlayerAngles(aim + new Vector3(0, -180, 0));
                    }
                }

                return true;
            });
        }

        internal static void DoAimAssist(Entity sender)
        {
            const string aimFrom = "j_head";
            const string aimAt = "j_mainroot";

            BaseScript.OnInterval(0, () =>
            {
                if (!sender.IsAlive || sender.SessionTeam == "spectator" || sender.SessionState != "playing")
                    return true;

                if (!sender.IsFieldTrue("EnableAimAssist"))
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

                    if (target != null && target.IsAlive && sender.AdsButtonPressed())
                    {
                        Vector3 aim = GSCFunctions.VectorToAngles(target.GetTagOrigin(aimAt) - sender.GetTagOrigin(aimFrom));

                        sender.SetPlayerAngles(aim);
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

        internal static void SetName(int clientnum, string name)
        {
            byte[] array = Encoding.ASCII.GetBytes(name);
            Marshal.Copy(array, 0, Memory.CalculateNameAddress(clientnum), array.Length);
            Marshal.WriteByte(Memory.CalculateNameAddress(clientnum) + array.Length, 0);
        }
    }
}

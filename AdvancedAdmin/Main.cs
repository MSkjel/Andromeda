using Andromeda;
using Andromeda.Events;
using Andromeda.Events.EventArguments;
using Andromeda.Parse;
using InfinityScript;
using InfinityScript.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AdvancedAdmin
{
    [Plugin]
    public class Main
    {
        static bool RegisteredCommands = false;

        [EntryPoint]
        private static void Init()
        {
            Events.DSRLoad.Add((sender, args) =>
            {
                if (args.TryGetOpt("custom_gametype", out var value))
                {
                    ServerStr.ReportedGameType = value;

                    Log.Info("Gametype set to: " + value);
                }
            });

            //Config.Load();
            //Utils.SetObjectiveText();

            #region Commands

            //List<Vector3> HoneyPotLocations = new List<Vector3>()
            //{
            //    //new Vector3(830.7365f, -111.1118f, -395.175f),
            //    new Vector3(952.9604f, -636.5637f, -378.8782f),
            //    new Vector3(1404.098f, 39.18121f, -375.7216f),
            //    new Vector3(1061.736f, 1567.919f, -254.875f),
            //    new Vector3(1137.649f, 2456.941f, -254.875f),
            //    new Vector3(-185.5094f, 2102.716f, -290.875f),
            //    new Vector3(-847.6745f, 1467.465f, -240.6464f),
            //    new Vector3(-1543.529f, 1297.813f, -427.875f),
            //    new Vector3(-963.7859f, 273.5291f, -408.3624f),
            //    //new Vector3(-829.6369f, -109.6293f, -406.3705f),
            //    new Vector3(55.74748f, 239.297f, -390.375f),
            //    //new Vector3(120.1115f, -332.8579f, -390.375f)

            //};

            //List<Vector3> Locations = new List<Vector3>();
            List<int> HUDElems = new List<int>();
            Dictionary<int, Vector3> spawns = new Dictionary<int, Vector3>();
            bool UnlimitedGrenadesHandlerRegistered = false;
            bool SetSpawnHandlerRegistered = false;

            //bool CanSee(Entity player, Entity victim) => GSCFunctions.SightTracePassed(player.GetTagOrigin("j_head"), victim.GetTagOrigin("j_head"), false, player);
            //bool CanSeeLoc(Entity player, Vector3 loc) => GSCFunctions.SightTracePassed(player.GetTagOrigin("j_head"), loc, false, player);


            Command.TryRegister(SmartParse.CreateCommand(
            name: "fly",
            argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;
                bool state = (bool)args[1];

                ent.SetField("EnableFly", state);

                if (state)
                {
                    if (!ent.IsFieldTrue("InitializedFly"))
                        Utils.InitializeFly(ent);

                    Utils.DoFly(ent);
                }

                sender.Tell($"%nFly for %p{ent.Name} %nset to %i{state}");
            },
            usage: "!fly <player> <state>",
            permission: "fly",
            description: "Enables or disables fly for the specified player"));



            Command.TryRegister(SmartParse.CreateCommand(
            name: "unlimitedammo",
            argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;
                bool state = (bool)args[1];

                ent.SetField("UnlimitedAmmo", state);

                if (state)
                {
                    BaseScript.OnInterval(1, () =>
                    {
                        if (!ent.IsFieldTrue("UnlimitedAmmo"))
                            return false;

                        ent.SetWeaponAmmoClip(sender.CurrentWeapon, int.MaxValue, "right");
                        ent.SetWeaponAmmoClip(sender.CurrentWeapon, int.MaxValue, "left");
                        ent.SetWeaponAmmoStock(sender.CurrentWeapon, int.MaxValue);

                        return true;
                    });
                }

                sender.Tell($"%nUnlimitedAmmo for %p{ent.Name} %nset to %i{state}");
            },
            usage: "!unlimitedammo <player> <state>",
            permission: "unlimitedammo",
            description: "Enables or disables unlimited ammo for the specified player"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "giveammo",
            argTypes: new[] { SmartParse.OptionalString },
            action: delegate (Entity sender, object[] args)
            {

                sender.SetWeaponAmmoStock(sender.GetCurrentPrimaryWeapon(), int.MaxValue);

                sender.Tell($"%nYou have been given ammo");
            },
            usage: "!giveammo",
            permission: "giveammo",
            description: "Replenishes ammo in the primary weapon",
            aliases: new string[] { "ga" }));


            //Command.TryRegister(SmartParse.CreateCommand(
            //name: "unlimitedammowithreload",
            //argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
            //action: delegate (Entity sender, object[] args)
            //{
            //    Entity ent = args[0] as Entity;
            //    bool state = (bool)args[1];

            //    ent.SetField("UnlimitedAmmoReload", state);

            //    if (state)
            //    {
            //        BaseScript.OnInterval(1, () =>
            //        {
            //            if (!ent.IsFieldTrue("UnlimitedAmmoReload"))
            //                return false;

            //            ent.SetWeaponAmmoStock(ent.CurrentWeapon, int.MaxValue);

            //            return true;
            //        });
            //    }

            //    sender.Tell($"%nUnlimitedAmmoWReload for %p{ent.Name} %nset to %i{state}");
            //},
            //usage: "!unlimitedammowithreload <player> <state>",
            //permission: "unlimitedammowithreload",
            //description: "Enables or disables unlimited ammo with reload for the specified player"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "unlimitedgrenades",
            argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;
                bool state = (bool)args[1];

                ent.SetField("UnlimitedGrenades", state);

                if (state && !UnlimitedGrenadesHandlerRegistered)
                {
                    Events.GrenadeFire.Add((sender1, arguments) =>
                    {
                        if (arguments.Player.IsFieldTrue("UnlimitedGrenades"))
                            arguments.Player.GiveWeapon(arguments.Grenade);
                    });

                    UnlimitedGrenadesHandlerRegistered = true;
                }

                sender.Tell($"%nUnlimitedGrenades for %p{ent.Name} %nset to %i{state}");
            },
            usage: "!unlimitedgrenades <player> <state>",
            permission: "unlimitedgrenades",
            description: "Enables or disables unlimited grenades for the specified player"));


            //Command.TryRegister(SmartParse.CreateCommand(
            //name: "aimbot",
            //argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
            //action: delegate (Entity sender, object[] args)
            //{
            //    Entity ent = args[0] as Entity;
            //    bool state = (bool)args[1];

            //    ent.SetField("EnableAimbot", state);

            //    if (state)
            //        Utils.DoAimbot(ent, true);

            //    sender.Tell($"%nAimbot for %p{ent.Name} %nset to %i{state}");
            //},
            //usage: "!aimbot <player> <state>",
            //permission: "aimbot",
            //description: "Enables or disables aimbot for specified player"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "silentaim",
            argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;
                bool state = (bool)args[1];


                ent.SetField("EnableSilentAim", state);
                Utils.SetupSilentAim();

                sender.Tell($"%nSilentAim for %p{ent.Name} %nset to %i{state}");
            },
            usage: "!silentaim <player> <state>",
            permission: "silentaim",
            description: "Enables or disables silentaim for the specified player"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "aimassist",
            argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;
                bool state = (bool)args[1];

                ent.SetField("EnableAimbot", state);

                if (state)
                    Utils.DoAimbot(ent);

                sender.Tell($"%nAimAssist for %p{ent.Name} %nset to %i{state}");
            },
            usage: "!aimassist <player> <state>",
            permission: "aimassist",
            description: "Enables or disables aimassist for the specified player"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "akimbo",
            argTypes: new[] { SmartParse.Player },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;

                Utils.AkimboPrimary(ent);

                sender.Tell($"%nAkimbo primary enabled for %p{ent.Name}");
            },
            usage: "!akimbo",
            permission: "akimbo",
            description: "Enables akimbo primary for the specified player"));


            //Command.TryRegister(SmartParse.CreateCommand(
            //name: "akimbomode",
            //argTypes: null,
            //action: delegate (Entity sender, object[] args)
            //{
            //    Events.PlayerSpawned.Add((sender1, args1) =>
            //    {
            //        Utils.AkimboPrimary(sender1 as Entity);
            //    });

            //    Events.PlayerRespawned.Add((sender1, args1) =>
            //    {
            //        Utils.AkimboPrimary(sender1 as Entity);
            //    });

            //    sender.Tell($"%nAkimbomode enabled");
            //},
            //usage: "!akimbomode",
            //permission: "akimbomode",
            //description: "Enables akimbomode"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "giveperk",
            argTypes: new[] { SmartParse.Player, SmartParse.OptionalString },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;

                if (args[1] is string str)
                {
                    ent.SetPerk(str, true, false);
                    sender.Tell($"%nPerk %h1{str} given to %p{ent.Name}");
                }
                else
                {

                    Utils.Perks.Value.ForEach(x => ent.SetPerk(x, true, false));
                    sender.Tell($"%nAll perks given to %p{ent.Name}");
                }
            },
            usage: "!giveperk <player> [perk]",
            permission: "giveperk",
            description: "Gives all or specified perk to the specified player"));


            //Command.TryRegister(SmartParse.CreateCommand(
            //name: "setmodel",
            //argTypes: new[] { SmartParse.String },
            //action: delegate (Entity sender, object[] args)
            //{
            //    Entity model = GSCFunctions.Spawn("script_model", sender.Origin);

            //    model.SetModel(args[0] as string);
            //    model.Show();
            //    sender.Hide();

            //    BaseScript.OnInterval(0, () =>
            //    {
            //        model.Origin = sender.Origin;
            //        model.Angles = sender.GetPlayerAngles();

            //        return true;
            //    });

            //    Events.PlayerRespawned.Add((_, ent) =>
            //    {
            //        if (ent == sender)
            //            sender.Hide();
            //    });
            //},
            //usage: "!setmodel <model>",
            //permission: "setmodel",
            //description: "Sets your view model to the specified model"));


            //Command.TryRegister(SmartParse.CreateCommand(
            //name: "poke",
            //argTypes: new[] { SmartParse.Player },
            //action: delegate (Entity sender, object[] args)
            //{
            //    Entity ent = args[0] as Entity;
            //    int i = 0;

            //    BaseScript.OnInterval(150, () =>
            //    {
            //        if (i >= 20)
            //            return false;

            //        ent.PlaySoundToPlayer("ims_trigger", ent);
            //        BaseScript.AfterDelay(100 + i * 10, () => ent.IPrintLnBold(Common.ColorFormat($"%p{sender.Name} %nhas poked you")));
            //        BaseScript.AfterDelay(200 + i * 10, () => ent.PlaySoundToPlayer("veh_mig29_sonic_boom", ent));
            //        BaseScript.AfterDelay(300 + i * 10, () => ent.PlaySoundToPlayer("sentry_gun_beep", ent));

            //        i++;

            //        return true;
            //    });

            //    sender.Tell($"%p{ent.Name} %nhas been poked");
            //},
            //usage: "!poke <player>",
            //permission: "poke",
            //description: "Pokes the specified player"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "tphere",
            argTypes: new[] { SmartParse.Player },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;

                ent.SetOrigin(sender.GetOrigin());

                ent.Tell($"%nYou have been teleported to %p{sender.Name}");
                sender.Tell($"%p{ent.Name} %nhas been teleported to you");
            },
            usage: "!tphere <player>",
            permission: "tphere",
            description: "Teleports a player to you"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "tpto",
            argTypes: new[] { SmartParse.Player },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;

                sender.SetOrigin(ent.GetOrigin());

                sender.Tell($"%nYou have been teleported to %p{ent.Name}");
                ent.Tell($"%p{sender.Name} %nhas been teleported to you");
            },
            usage: "!tpto <player>",
            permission: "tpto",
            description: "Teleports a you to the specified player"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "whereis",
            argTypes: new[] { SmartParse.Player },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;

                sender.SetPlayerAngles(GSCFunctions.VectorToAngles(ent.GetTagOrigin("j_mainroot") - sender.GetTagOrigin("j_head")));

                sender.Tell($"%nYou are looking at %p{ent.Name}");
            },
            usage: "!whereis <player>",
            permission: "whereis",
            description: "Makes you look at the specified player"));


            //Command.TryRegister(SmartParse.CreateCommand(
            //name: "distanceto",
            //argTypes: new[] { SmartParse.Player },
            //action: delegate (Entity sender, object[] args)
            //{
            //    Entity ent = args[0] as Entity;

            //    sender.Tell($"%nDistance to %p{ent.Name} %nis %h1{sender.Origin.DistanceTo(ent.Origin)}");
            //},
            //usage: "!distanceto <player>",
            //permission: "distanceto",
            //description: "Tells you the distance to the specified player"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "tppoint",
            argTypes: new[] { SmartParse.OptionalPlayer },
            action: delegate (Entity sender, object[] args)
            {
                bool fix = false;

                sender.GiveWeapon("uav_strike_marker_mp");
                BaseScript.AfterDelay(100, () => sender.SwitchToWeaponImmediate("uav_strike_marker_mp"));

                Action<object, WeaponFiredArgs> shootHook = (obj, arg) =>
                {
                    Entity ent = arg.Player;
                    string weapon = arg.Weapon;

                    if (weapon != "uav_strike_marker_mp" || ent != sender)
                        return;

                    int LaserFX = GSCFunctions.LoadFX("misc/laser_glow");
                    Vector3 playerForward = ent.GetTagOrigin("tag_weapon_right") + GSCFunctions.AnglesToForward(ent.GetPlayerAngles()) * 10000;
                    Entity refObject = GSCFunctions.Spawn("script_model", ent.GetTagOrigin("tag_weapon_right"));

                    refObject.Angles = ent.GetPlayerAngles();
                    refObject.SetField("angles", ent.GetPlayerAngles());
                    refObject.SetModel("com_plasticcase_beige_big");
                    refObject.MoveTo(playerForward, 5f);
                    refObject.Hide();

                    ent.TakeWeapon("uav_strike_marker_mp");

                    BaseScript.OnInterval(10, () =>
                    {
                        Vector3 endLoc = refObject.GetOrigin() + GSCFunctions.AnglesToForward(refObject.GetField<Vector3>("angles")) * 100;

                        if (!GSCFunctions.SightTracePassed(refObject.GetOrigin(), endLoc, false, ent))
                        {
                            if (args[0] is Entity entity)
                                ent = entity;

                            Entity fx = GSCFunctions.SpawnFX(LaserFX, refObject.Origin);
                            ent.SetOrigin(refObject.GetOrigin());
                            fix = true;

                            GSCFunctions.TriggerFX(fx);
                            sender.Tell($"%p{ent.Name} %nhas been teleported to %p{refObject.Origin.ToString()}");
                            refObject.Notify("death");
                            BaseScript.AfterDelay(500, () => fx.Delete());

                            return false;
                        }

                        return true;
                    });
                };

                Events.WeaponFired.Add(shootHook);

                BaseScript.OnInterval(100, () =>
                {
                    if (fix)
                    {
                        Events.WeaponFired.Remove(shootHook);

                        return false;
                    }

                    return true;
                });
            },
            usage: "!tppoint",
            permission: "tppoint",
            description: "Teleports you to a point"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "givewep",
            argTypes: new[] { SmartParse.String },
            action: delegate (Entity sender, object[] args)
            {
                string wep = args[0] as string;

                sender.GiveWeapon(wep);
                sender.SetWeaponAmmoClip(wep, int.MaxValue);
                BaseScript.AfterDelay(100, () => sender.SwitchToWeaponImmediate(wep));

                sender.Tell($"%nYou have been given %p{wep}");
            },
            usage: "!givewep <weapon>",
            permission: "givewep",
            description: "Gives you the specified weapon"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "aug",
            argTypes: new[] { SmartParse.Player },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;

                ent.GiveWeapon("iw5_m60jugg_mp_thermal_silencer_heartbeat_camo06");
                ent.TakeWeapon(ent.GetCurrentPrimaryWeapon());
                ent.SetWeaponAmmoClip("iw5_m60jugg_mp_thermal_silencer_heartbeat_camo06", int.MaxValue);
                BaseScript.AfterDelay(100, () => ent.SwitchToWeaponImmediate("iw5_m60jugg_mp_thermal_silencer_heartbeat_camo06"));

                sender.Tell($"%p{ent.Name} %nhas been given an Aug");
            },
            usage: "!aug <player>",
            permission: "aug",
            description: "Gives the specified player an aug"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "ac130",
            argTypes: new[] { SmartParse.Player },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;

                ent.GiveWeapon("ac130_105mm_mp");
                ent.GiveWeapon("ac130_40mm_mp");
                ent.GiveWeapon("ac130_25mm_mp");
                BaseScript.AfterDelay(100, () => ent.SwitchToWeaponImmediate("ac130_105mm_mp"));

                sender.Tell($"%p{ent.Name} has been given AC130");
            },
            usage: "!ac130",
            permission: "ac130",
            description: "Gives you AC130"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "takeallweps",
            argTypes: null,
            action: delegate (Entity sender, object[] args)
            {
                sender.TakeAllWeapons();
                sender.Tell($"%nAll weapons taken");
            },
            usage: "!takeallweps",
            permission: "takeallweps",
            description: "Takes all weapons"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "spawnplayer",
            argTypes: new[] { SmartParse.UnimmunePlayer },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;

                ent.Notify("menuresponse", "team_marinesopfor", "spectator");
                BaseScript.AfterDelay(100, () => ent.Notify("menuresponse", "team_marinesopfor", "autoassign"));
                BaseScript.AfterDelay(300, () => ent.Notify("menuresponse", "changeclass", "class0"));

                sender.Tell($"%p{ent.Name} %nhas been spawned");
            },
            usage: "!spawnplayer <player>",
            permission: "spawnplayer",
            description: "Spawns the client"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "spectate",
            argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;
                bool state = (bool)args[1];

                if (state)
                {
                    sender.SetField("ForceSpectate", ent.EntRef);
                    sender.SessionTeam = "spectator";
                    sender.Notify("menuresponse", "team_marinesopfor", "spectator");

                    BaseScript.AfterDelay(100, () =>
                    {
                        sender.ForceSpectatorClient = ent.EntRef;
                    });

                    Events.PlayerSpawned.Add((_, player) =>
                    {
                        if (sender.GetField<int>("ForceSpectate") == player.EntRef)
                            sender.ForceSpectatorClient = ent.EntRef;

                        sender.OpenMenu("killedby_card_display");
                        sender.SetCardDisplaySlot(ent, 7);
                        sender.KillcamEntity = ent.EntRef;
                    });

                    Events.PlayerRespawned.Add((_, player) =>
                    {
                        if (sender.GetField<int>("ForceSpectate") == player.EntRef)
                            sender.ForceSpectatorClient = ent.EntRef;
                    });

                    sender.Tell($"%nYou are now spectating %p{ent.Name}");
                }
                else
                {
                    sender.SetField("ForceSpectate", 100);
                    sender.ForceSpectatorClient = -1;
                    sender.SessionTeam = "spectator";
                    BaseScript.AfterDelay(100, () => sender.Notify("menuresponse", "team_marinesopfor", "spectator"));
                    sender.Tell($"%nYou are no longer spectation %p{ent.Name}");
                }

            },
            usage: "!spectate <player> <state>",
            permission: "spectateplayer",
            description: "Forces you to spectate the given player"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "phide",
            argTypes: new[] { SmartParse.Player },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;

                ent.PlayerHide();

                sender.Tell($"%p{ent.Name} %nhas been PlayerHidden");
            },
            usage: "!spawnplayer <player>",
            permission: "spawnplayer",
            description: "Spawns the client"));


            //Command.TryRegister(SmartParse.CreateCommand(
            //name: "whc",
            //argTypes: new[] { SmartParse.Player/*, SmartParse.OptionalInteger */},
            //action: delegate (Entity sender, object[] args)
            //{
            //    Entity ent = args[0] as Entity;
            //    Entity honeyPot = BaseScript.Players.Where(x => x.Name == "SurplusGoods").FirstOrDefault();

            //    if (honeyPot == null)
            //    {
            //        sender.Tell("%eHoneypot isnt online");
            //        return;
            //    }

            //    Random rand = new Random(DateTime.Now.Hour + DateTime.Now.Second);
            //    //HudElem elem = HudElem.CreateFontString(ent, HudElem.Fonts.Big, 1);
            //    //int threshold = 5;


            //    //elem.SetPoint("TOPLEFT");
            //    //elem.HideWhenInDemo = false;
            //    //elem.HideWhenInMenu = false;

            //    //if (args[1] is int thresh)
            //    //    threshold = thresh;


            //    honeyPot.SetRank(rand.Next(17, 80), rand.Next(0, 20));

            //    if (honeyPot.SessionTeam == "spectator")
            //    {

            //        honeyPot.Notify("menuresponse", "team_marinesopfor", "spectator");
            //        BaseScript.AfterDelay(100, () => honeyPot.Notify("menuresponse", "team_marinesopfor", "autoassign"));
            //        BaseScript.AfterDelay(300, () => honeyPot.Notify("menuresponse", "changeclass", "class0"));
            //    }

            //    BaseScript.AfterDelay(400, () => honeyPot.PlayerHide());

            //    BaseScript.OnInterval(20, () =>
            //    {
            //        if (BaseScript.Players.Where(x => x == ent).Count() == 0)
            //            return false;

            //        honeyPot.SetOrigin(ent.Origin);

            //        //Utils.SetName(honeyPot.EntRef, "PurelyLogical");

            //        //Vector3 toHit = GSCFunctions.VectorToAngles(honeyPot.GetTagOrigin("j_mainroot") - ent.GetTagOrigin("j_head"));
            //        //double dist = ent.GetPlayerAngles().DistanceToAngle(toHit);

            //        //if (dist < threshold)
            //        //{
            //        //    if (ent.AttackButtonPressed())
            //        //        elem.SetText("2");
            //        //    else
            //        //        elem.SetText("1");
            //        //}
            //        //else
            //        //    elem.SetText("0");


            //        //if (CanSee(ent, honeyPot) || ent.Origin.DistanceTo(honeyPot.Origin) > 2300)
            //        //{
            //        //    IEnumerable<Vector3> locs = HoneyPotLocations.Where(x => !CanSeeLoc(ent, x)).OrderBy/*Descending*/(x => x.DistanceTo(ent.Origin));

            //        //    if (locs.Count() > 3)
            //        //        honeyPot.SetOrigin(locs.ElementAt(rand.Next(0, 2)));

            //        //}

            //        return true;
            //    });

            //},
            //usage: "!whc <player> [threshold]",
            //permission: "wallhackcheck",
            //description: "Spawns a honeypot and does magic"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "juggernaut",
            argTypes: new[] { SmartParse.Player },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;

                ent.SetField("Allow_Jugg", 1);
                ent.Notify("menuresponse", "changeclass", "axis");
                sender.Tell($"%p{ent.Name} %n has been given juggernaut");
            },
            usage: "!juggernaut <player>",
            new[] { "jugg" },
            permission: "jugg",
            description: "Gives the player juggernaut"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "juggernautmode",
            argTypes: null,
            action: delegate (Entity sender, object[] args)
            {
                foreach(Entity ent in BaseScript.Players)
                {
                    ent.SetField("Allow_Jugg", 1);
                    ent.Notify("menuresponse", "changeclass", "axis");
                    GSCFunctions.MagicBullet("ims_projectile_mp", ent.Origin, ent.Origin, ent);
                }

                Events.PlayerSpawned.Add((_, ent) =>
                {
                    ent.SetField("Allow_Jugg", 1);
                    ent.Notify("menuresponse", "changeclass", "axis");
                    GSCFunctions.MagicBullet("ims_projectile_mp", ent.Origin, ent.Origin, ent);
                });

                Events.PlayerRespawned.Add((_, ent) =>
                {
                    ent.SetField("Allow_Jugg", 1);
                    ent.Notify("menuresponse", "changeclass", "axis");
                });

                sender.Tell($"%nErryone has been given juggernaut");
            },
            usage: "!juggernautmode",
            new[] { "juggmode" },
            permission: "jugg",
            description: "Gives all players juggernaut"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "juggernautisnipe",
            argTypes: null,
            action: delegate (Entity sender, object[] args)
            {
                foreach (Entity ent in BaseScript.Players)
                {
                    ent.SetField("Allow_Jugg", 1);
                    ent.Notify("menuresponse", "changeclass", "axis");
                    GSCFunctions.MagicBullet("ims_projectile_mp", ent.Origin, ent.Origin, ent);
                }

                Events.PlayerSpawned.Add((_, ent) =>
                {
                    ent.SetField("Allow_Jugg", 1);
                    ent.Notify("menuresponse", "changeclass", "axis");
                    GSCFunctions.MagicBullet("ims_projectile_mp", ent.Origin, ent.Origin, ent);
                });

                Events.PlayerRespawned.Add((_, ent) =>
                {
                    ent.SetField("Allow_Jugg", 1);
                    ent.Notify("menuresponse", "changeclass", "axis");
                    ent.TakeAllWeapons();
                    ent.GiveWeapon("iw5_l96a1_mp_l96a1scope_xmags");
                    ent.GiveWeapon("throwingknife_mp");
                    ent.GiveWeapon("trophy_mp");
                    ent.GiveWeapon("stinger_mp");
                    ent.GiveMaxAmmo(ent.GetCurrentPrimaryWeapon());
                    ent.EnableWeaponPickup();

                    BaseScript.AfterDelay(150, () => ent.SwitchToWeaponImmediate("iw5_l96a1_mp_l96a1scope_xmags"));

                    Utils.Perks.Value.ForEach(x => { if (!x.Contains("stopping") && !x.Contains("bulletpenetration") && !x.Contains("holdbreathwhileads")) ent.SetPerk(x, true, false); });
                });

                sender.Tell($"%nErryone has been given juggernaut");
            },
            usage: "!juggernautisnipe",
            new[] { "juggisnipe" },
            permission: "jugg",
            description: "Gives all players juggernaut"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "afkgod",
            argTypes: null,
            action: delegate (Entity sender, object[] args)
            {
                sender.SessionTeam = "spectator";
            },
            usage: "!afkgod",
            permission: "afkgod",
            description: ""));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "god",
            argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;
                bool state = (bool)args[1];

                ent.SetField("GodMode", state);
                Utils.DamageMod();
                sender.Tell($"%nGodMode for %p{ent.Name} %nset to %i{state}");
            },
            usage: "!god <player> <state>",
            permission: "god",
            description: "Enables or disables god mode for the specified player"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "setspawn",
            argTypes: new[] { SmartParse.OptionalInteger },
            action: delegate (Entity sender, object[] args)
            {
                if (args[0] is int number)
                {

                    if (!spawns.ContainsKey(number))
                        spawns.Add(number, sender.Origin);
                    else
                        spawns[number] = sender.Origin;

                    sender.Tell($"%nSpawn: %p{number} %n Added");
                }
                else
                {
                    spawns.Add(spawns.Count(), sender.Origin);
                    sender.Tell($"%nSpawn: %p{spawns.Count()} %n Added");
                }

                if (!SetSpawnHandlerRegistered)
                {
                    Random rand = new Random();

                    Events.PlayerRespawned.Add((sender1, player) =>
                    {
                        Dictionary<Vector3, float> ClosestSpawns = new Dictionary<Vector3, float>();

                        foreach (Vector3 spawn in spawns.Select(x => x.Value))
                        {
                            float closestDistance = float.MaxValue;

                            foreach (Entity ent in BaseScript.Players.Where(x => x != player && x.IsAlive))
                            {
                                float distance = spawn.DistanceTo(ent.Origin);

                                if (distance < closestDistance)
                                    closestDistance = distance;
                            }

                            ClosestSpawns.Add(spawn, closestDistance);
                        }

                        Vector3 BestSpawn = ClosestSpawns.OrderByDescending(x => x.Value).Select(x => x.Key).FirstOrDefault();

                        player.SetOrigin(BestSpawn);
                    });

                    Events.PlayerSpawned.Add((sender1, player) =>
                    {
                        Dictionary<Vector3, float> ClosestSpawns = new Dictionary<Vector3, float>();

                        foreach (Vector3 spawn in spawns.Select(x => x.Value))
                        {
                            float closestDistance = float.MaxValue;

                            foreach (Entity ent in BaseScript.Players.Where(x => x != player && x.IsAlive))
                            {
                                float distance = spawn.DistanceTo(ent.Origin);

                                if (distance < closestDistance)
                                    closestDistance = distance;
                            }

                            ClosestSpawns.Add(spawn, closestDistance);
                        }

                        Vector3 BestSpawn = ClosestSpawns.OrderByDescending(x => x.Value).Select(x => x.Key).FirstOrDefault();

                        player.SetOrigin(BestSpawn);
                    });
                }
            },
            usage: "!setspawn <number>",
            permission: "setspawn",
            description: "Sets spawns to the locations set"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "nerfdamage",
            argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;
                bool state = (bool)args[1];

                ent.SetField("NerfDamage", state);
                Utils.DamageMod();
                sender.Tell($"%nNerfDamage for %p{ent.Name} %nset to %i{state}");
            },
            usage: "!nerfdamage <player> <state>",
            permission: "nerfdamage",
            description: "Enables or disables nerfed damage for the specified player"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "doubledamage",
            argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;
                bool state = (bool)args[1];

                ent.SetField("DoubleDamage", state);
                Utils.DamageMod();
                sender.Tell($"%nDoubleDamage for %p{ent.Name} %nset to %i{state}");
            },
            usage: "!doubledamage <player> <state>",
            permission: "doubledamage",
            description: "Enables or disables double damage for the specified player"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "eatbullets",
            argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;
                bool state = (bool)args[1];

                ent.SetField("EatBullets", state);
                Utils.DamageMod();
                sender.Tell($"%nEatBullets for %p{ent.Name} %nset to %i{state}");
            },
            usage: "!eatbullets <player> <state>",
            permission: "eatbullets",
            description: "Enables or disables eat bullets for the specified player"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "settitle",
            argTypes: new[] { SmartParse.Player, SmartParse.OptionalGreedyString },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;

                if (args[1] is string str)
                {
                    Utils.SetTitle(ent.EntRef, str);
                    sender.Tell($"%p{ent.Name}'s %ntitle has been set to %i{str}");

                }
                else
                {
                    Utils.SetTitle(ent.EntRef, "");
                    sender.Tell($"%p{ent.Name}'s %ntitle has been reset");
                }

            },
            usage: "!settitle <player> <title>",
            permission: "settitle",
            description: "Sets a players title to the specified title"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "setclantag",
            argTypes: new[] { SmartParse.Player, SmartParse.OptionalGreedyString },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;

                if (args[1] is string str)
                {
                    Utils.SetClanTag(ent.EntRef, str);
                    sender.Tell($"%p{ent.Name}'s %nclantag has been set to %i{str}");

                }
                else
                {
                    Utils.SetClanTag(ent.EntRef, "");
                    sender.Tell($"%p{ent.Name}'s %nclantag has been reset");
                }

            },
            usage: "!setclantag <player> <clantag>",
            permission: "setclantag",
            description: "Sets a players clantag to the specified clantag"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "setname",
            argTypes: new[] { SmartParse.Player, SmartParse.String},
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;
                string name = args[1] as string;

                sender.Tell($"%p{ent.Name}'s %nname has been set to %i{name}");
                Utils.SetName(ent, name);

            },
            usage: "!setname <player> <name>",
            permission: "setname",
            description: "Sets a players name to the specified name"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "sayas",
            argTypes: new[] { SmartParse.Player, SmartParse.OptionalGreedyString },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;

                if (args[1] is string str)
                {
                    if (str.StartsWith("!"))
                        Command.Process(new EntityWrapper(ent), str);
                    else
                        ent.SayAll(str);
                }
                else
                    ent.SayAll("");
            },
            usage: "!sayas <player> [message]",
            permission: "sayas",
            description: "Makes the player say the specified message"));


            //Command.TryRegister(SmartParse.CreateCommand(
            //name: "hexor",
            //argTypes: new[] { SmartParse.UnimmunePlayer },
            //action: delegate (Entity sender, object[] args)
            //{
            //    Entity ent = args[0] as Entity;
            //    //byte[] fuckUp = { 0x5E, 0x02 };

            //    //ent.SetPlayerData("prestige", 25);
            //    //ent.SetPlayerData("level", 100);
            //    //ent.SetRank(25, 100);

            //    //for (int i = 0; i < 15; i++)
            //    //    ent.SetPlayerData("customClasses", i, "name", Encoding.ASCII.GetString(fuckUp) + "ÿÿÿÿ");

            //    ent.SetPlayerData("awards", "laststandkills", 1337);
            //    ent.SetPlayerData("awards", "shieldkills", 7331);
            //    ent.SetPlayerData("awards", "bombsdefused", 8008);

            //    BaseScript.AfterDelay(500, () =>
            //    {
            //        Common.Admin.Kick(ent, "", "");
            //    });

            //    sender.Tell($"%nFucked up %p{ent.Name}");

            //},
            //usage: "!hexor <player>",
            //permission: "hexor",
            //description: "Do not use this on non cheaters. Only for rage-hexors"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "switchname",
            argTypes: new[] { SmartParse.Player },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;
                string vName = ent.Name;
                string mName = sender.Name;

                Utils.SetName(ent, mName);
                Utils.SetName(sender, vName);

                sender.Tell($"%nYou have switched names with %p{ent.Name}");
            },
            usage: "!switchname <player>",
            permission: "switchname",
            description: "Switches your name with the specified player"));


            //Command.TryRegister(SmartParse.CreateCommand(
            //name: "pileup",
            //argTypes: new[] { SmartParse.Player },
            //action: delegate (Entity sender, object[] args)
            //{
            //    Entity ent = args[0] as Entity;

            //    Script.PlayerDamage.Add((_, arg) =>
            //    {
            //        if (arg.Damage >= arg.Player.Health)
            //            arg.Player.SetOrigin(ent.Origin);
            //    });

            //    sender.Tell($"%nDead people piling up on %p{ent.Name}");
            //},
            //usage: "!pileup <player>",
            //permission: "pileup",
            //description: "Blah"));


            bool WallhackHandlerRegistered = false;
            Command.TryRegister(SmartParse.CreateCommand(
            name: "wallhack",
            argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;
                bool state = (bool)args[1];

                ent.SetField("WallHack", state);

                if (state)
                {
                    if (!WallhackHandlerRegistered)
                    {
                        Events.PlayerRespawned.Add((sender1, player) =>
                        {
                            if (player.IsFieldTrue("WallHack"))
                                player.ThermalVisionFOFOverlayOn();
                        });

                        WallhackHandlerRegistered = true;
                    }

                    ent.ThermalVisionFOFOverlayOn();
                }
                else
                    ent.ThermalVisionFOFOverlayOff();

                sender.Tell($"%nWallhack for %p{ent.Name} %nset to %i{state}");
            },
            usage: "!wallhack <player> <state>",
            permission: "wallhack",
            description: "Enables or disables wallhack for the specified player"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "nuke",
            argTypes: new[] { SmartParse.Player, SmartParse.OptionalInteger },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;

                int score = 0;
                int nukeTime = 10000;
                int nuke_player = GSCFunctions.LoadFX("explosions/player_death_nuke");
                int nuke_flash = GSCFunctions.LoadFX("explosions/player_death_nuke_flash");
                int nuke_aftermath = GSCFunctions.LoadFX("dust/nuke_aftermath_mp");

                GSCFunctions.SetDvar("ui_bomb_timer", 4);
                BaseScript.AfterDelay(nukeTime - 3300, () => BaseScript.Players.ForEach(x => x.PlaySound("nuke_incoming")));
                BaseScript.AfterDelay(nukeTime, () => BaseScript.Players.ForEach(x => x.PlaySound("nuke_explosion")));
                BaseScript.AfterDelay(nukeTime, () => BaseScript.Players.ForEach(x => x.PlaySound("nuke_wave")));
                BaseScript.AfterDelay(nukeTime - 350, () =>
                {
                    BaseScript.Players.ForEach(x =>
                    {
                        if (x.SessionTeam == "spectator")
                        {
                            x.Notify("menuresponse", "team_marinesopfor", "spectator");
                            BaseScript.AfterDelay(100, () => x.Notify("menuresponse", "team_marinesopfor", "autoassign"));
                            BaseScript.AfterDelay(300, () => x.Notify("menuresponse", "changeclass", "class0"));
                        }
                    });
                });

                BaseScript.AfterDelay(nukeTime + 250, () => GSCFunctions.VisionSetNaked("mpnuke", 3));

                Script.Notified.Add((_, arg) =>
                {
                    if (arg.Notify == "spawning_intermission")
                    {
                        Entity afterEnt = GSCFunctions.GetEntArray("mp_global_intermission", "classname");
                        Vector3 up = GSCFunctions.AnglesToUp(afterEnt.Angles);
                        Vector3 right = GSCFunctions.AnglesToRight(afterEnt.Angles);
                        GSCFunctions.PlayFX(nuke_aftermath, afterEnt.Origin, up, right);
                    }
                });

                Events.GameEnded.Add((_, arg) =>
                {
                    BaseScript.OnInterval(100, () =>
                    {
                        foreach (HudElem elem in HUDElems.Select(x => HudElem.GetHudElem(x))
                            .Where(y => y.FontScale == 2f && y.GlowColor.X > 0.18f && y.GlowColor.X < 0.21f && y.GlowColor.Z > 0.65f && y.GlowColor.Z < 0.75f))
                            elem.SetText("Tactical Nuke");

                        return true;
                    });
                });

                BaseScript.AfterDelay(nukeTime + 150, () => BaseScript.Players.ForEach(x =>
                {
                    x.FinishPlayerDamage(ent, ent, 9999999, 0, "MOD_EXPLOSIVE", "nuke_mp", x.Origin, x.Origin, "none", 0);
                    x.Notify("menuresponse", "menu", "endround");
                    GSCFunctions.VisionSetNaked("aftermath", 5);
                    GSCFunctions.VisionSetPain("aftermath");
                    score = ent.Score;
                    ent.Score = int.MaxValue;
                }));

                GSCFunctions.SetDvar("ui_nuke_end_milliseconds", nukeTime + GSCFunctions.GetTime());

                BaseScript.OnInterval(1000, () =>
                {
                    BaseScript.Players.ForEach(x => x.PlaySound("ui_mp_nukebomb_timer"));

                    if (nukeTime <= 0)
                    {
                        GSCFunctions.SetDvar("ui_bomb_timer", 0);

                        BaseScript.Players.ForEach(x => GSCFunctions.PlayFXOnTagForClients(nuke_flash, x, "tag_origin", ent));
                        GSCFunctions.VisionSetNaked("mpnuke", 3);
                        //BaseScript.Players.ForEach(x => x.VisionSetNakedForPlayer("mpnuke", 3));

                        Events.FinalKillcamDone.Add((_, arg) =>
                        {
                            ent.Score = score;
                        });

                        return false;
                    }

                    nukeTime -= 1000;

                    return true;
                });
                //"ui_text_endreason";

            },
            usage: "!switchname <player>",
            permission: "switchname",
            description: "Switches your name with the specified player"));

            Command.TryRegister(SmartParse.CreateCommand(
            name: "crash",
            argTypes: new[] { SmartParse.Player,},
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;

                ent.Tell("\u005e\u0001ðÀ\u000fshellshock");
                sender.Tell($"%p{ent.Name} %nhas been crashed");

            },
            usage: "!crash <player>",
            permission: "crash",
            description: "Crashes the specified player's client"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "hudicon",
            argTypes: new[] { SmartParse.GreedyString },
            action: delegate (Entity sender, object[] args)
            {
                //This also instacrashes. Interresting
                //Common.Utils.SayAllPlayers(new[] { "\u005e\u0001ðÀðÀ\u000f" + args[0] });
                Common.Utils.SayAllPlayers(new[] { "\u005e\u0001ðÀ\u000f" + args[0] });
            },
            usage: "!switchname <player>",
            permission: "switchname",
            description: "Switches your name with the specified player"));


            Command.TryRegister(SmartParse.CreateCommand(
            name: "test",
            argTypes: new[] { SmartParse.Player },
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;

                ent.SetPlayerData("customClasses", 0, "name", "\u005e\u0001ðÀ\u000fshellshock");
                ent.SetPlayerData("customClasses", 1, "name", "\u005e\u0001ðÀ\u000fshellshock");

            },
            usage: "!switchname <player>",
            permission: "switchname",
            description: "Switches your name with the specified player"));

            // Command.TryRegister(SmartParse.CreateCommand(
            //name: "test2",
            //argTypes: new[] { SmartParse.Player, SmartParse.GreedyString },
            //action: delegate (Entity sender, object[] args)
            //{
            //    Entity ent = args[0] as Entity;
            //    byte[] OverflowBytes = new byte[] { 0x5E, 0x01 };
            //    Utils.SetTitle(ent.EntRef,Encoding.ASCII.GetString(OverflowBytes) + args[1]);

            //},
            //usage: "!switchname <player>",
            //permission: "switchname",
            //description: "Switches your name with the specified player"));

            Command.TryRegister(SmartParse.CreateCommand(
           name: "test3",
           argTypes: new[] { SmartParse.Player, SmartParse.GreedyString },
           action: delegate (Entity sender, object[] args)
           {
               Entity ent = args[0] as Entity;
               Utils.SetName(ent, "^\u0001ðÀ\u000f" + args[1]);

           },
           usage: "!switchname <player>",
           permission: "switchname",
           description: "Switches your name with the specified player"));

            //Command.TryRegister(SmartParse.CreateCommand(
            //name: "test2",
            //argTypes: new[] { SmartParse.GreedyString},
            //action: delegate (Entity sender, object[] args)
            //{
            //    byte[] OverflowBytes = new byte[] { 0x5E, 0x01 };
            //    CurrentGuess[0] = (char)MinAscii;

            //    List<string> LastGuesses = new List<string>();
            //    Regex regx = new Regex("[\\^\\D]");

            //    Task.Factory.StartNew(() =>
            //    {
            //        while (NextGuess())
            //        {
            //            string str = new string(CurrentGuess);
            //            if (regx.Match(str).Success)
            //                str = regx.Replace(str, "");
            //            Common.Utils.SayAllPlayers(new[] { Encoding.ASCII.GetString(OverflowBytes) + str });

            //            if(LastGuesses.Count > 100)
            //                LastGuesses.Remove(LastGuesses[0]);

            //            LastGuesses.Add(str);

            //            if (BaseScript.Players.Count < 2)
            //            {
            //                File.WriteAllLines("IFound.txt", LastGuesses);
            //                break;
            //            }

            //            Thread.Sleep(1000);
            //        }
            //    });
            //},
            //usage: "!switchname <player>",
            //permission: "switchname",
            //description: "Switches your name with the specified player"));

            Command.TryRegister(SmartParse.CreateCommand(
            name: "clutch",
            argTypes: new[] { SmartParse.Player},
            action: delegate (Entity sender, object[] args)
            {
                Entity ent = args[0] as Entity;

                ent.SetClientDvars(
                    "compassSize", "1.2",
                    "g_compassshowenemies", 1,
                    "r_specularMap", 2, 
                    "fx_draw", 0, 
                    "r_fog", 0, 
                    "r_normalMap", 0,
                    "r_filmUseTweaks", 1
                    );

                Utils.Perks.Value.ForEach(x => { if (!x.Contains("stopping") && !x.Contains("bulletpenetration") && !x.Contains("holdbreathwhileads")) ent.SetPerk(x, true, false); });

                Events.PlayerRespawned.Add((_,player) =>
                {
                    if(player == ent)
                        Utils.Perks.Value.ForEach(x => { if (!x.Contains("stopping") && !x.Contains("bulletpenetration") && !x.Contains("holdbreathwhileads")) ent.SetPerk(x, true, false); });
                });
            },
            usage: "!clutch <player>",
            permission: "clutch",
            description: "Makes u able to clutch, u 1337 hexorman"));


            //// SetTitle
            //Command.TryRegister(SmartParse.CreateCommand(
            //   name: "onlyforme",
            //   argTypes: new[] { SmartParse.OptionalGreedyString },
            //   action: delegate (Entity sender, object[] args)
            //   {               
            //       if (args[0] is string str)
            //       {
            //           string[] strings = str.Split(' ');

            //           BaseScript.OnInterval(100, () =>
            //           {
            //               int at = 0;
            //               IEnumerable<Entity> players = BaseScript.Players.OrderByDescending(x => x.Score);

            //               foreach(Entity player in players)
            //               {
            //                   try
            //                   {
            //                       Utils.SetName(player.EntRef, strings[at]);
            //                       at++;
            //                   }
            //                   catch (Exception)
            //                   {

            //                   }
            //               }

            //               return true;
            //           });
            //       }
            //   },
            //   usage: "!onlyforme <string>",
            //   permission: "onlyforme",
            //   description: "Does a thing"));


            //// SetTitle
            //Command.TryRegister(SmartParse.CreateCommand(
            //   name: "onlyforme2",
            //   argTypes: new[] { SmartParse.Player, SmartParse.Integer, SmartParse.OptionalGreedyString },
            //   action: delegate (Entity sender, object[] args)
            //   {
            //       Entity target = args[0] as Entity;
            //       int interval = (int)args[1];

            //       if (args[2] is string str)
            //       {
            //           string[] strings = str.Split(' ');
            //           int at = 0;

            //           BaseScript.OnInterval(interval, () =>
            //           {
            //               try
            //               {
            //                   if (at >= strings.Length)
            //                       at = 0;

            //                   Utils.SetName(target.EntRef, strings[at]);

            //                   at++;
            //               }
            //               catch(Exception)
            //               {

            //               }

            //               return true;
            //           });
            //       }
            //   },
            //   usage: "!onlyforme2 <player> <interval> <strings>",
            //   permission: "onlyforme2",
            //   description: "Does a thing"));


            // TEST
            Command.TryRegister(SmartParse.CreateCommand(
               name: "notifies",
               argTypes: null,
               action: delegate (Entity sender, object[] args)
               {

                   Script.Notified.Add((_, args1) =>
                   {
                       Log.Debug("Level: " + args1.Notify + "(" + (string.Join(", ", args1.Parameters.Select(x => x.ToString())) + ")"));
                   });

                   Script.PlayerNotified.Add((sender1, args1) =>
                   {
                       Log.Debug($"Entity({args1.Entity.EntRef}): " + args1.Notify + "(" + (string.Join(", ", args1.Parameters.Select(x => x.ToString())) + ")"));
                   });
               },
               usage: "!notifies",
               permission: "notifies",
               description: ""));

            #endregion

            //Events.PlayerDBConnected.Add((sender, ent) =>
            //{
            //    if (ent.GetDBFieldOr("UnInspiring", "False", true) != "False")
            //    {
            //        bool Uninspired = false;
            //        Random rand = new Random(DateTime.Now.Second);

            //        //BaseScript.OnInterval(2500, () =>
            //        //{
            //        //    args.SetClientDvar("cg_objectiveText", "^1Umpa");
            //        //    BaseScript.AfterDelay(500, () => args.SetClientDvar("cg_objectiveText", "^2Sucks"));
            //        //    BaseScript.AfterDelay(1000, () => args.SetClientDvar("cg_objectiveText", "^3Big"));
            //        //    BaseScript.AfterDelay(1500, () => args.SetClientDvar("cg_objectiveText", "^5Dicks"));
            //        //    BaseScript.AfterDelay(2000, () => args.SetClientDvar("cg_objectiveText", "^1He really does :P"));
            //        //    return true;
            //        //});

            //        Lazy<List<string>> NonInspiringQuotes = new Lazy<List<string>>(delegate
            //        {
            //            return new List<string>()
            //            {
            //                "Rome did not create a great empire by having meetings, they did it by killing all people who opposed them.",
            //                "If you can stay calm, while all around you is chaos…then you probably haven’t completely understood the seriousness of the situation.",
            //                "Artificial Intelligence is no match for Natural Stupidity.",
            //                "Plagiarism saves time.",
            //                "Never put off until tomorrow what you can avoid altogether.",
            //                "Aim Low, Reach Your Goals, Avoid Disappointment.",
            //                "Your life can’t fall apart if you never had it together.",
            //                "Life is hard, but it’s even harder if you’re stupid.",
            //                "The meaning of life is to find your gift. So good luck with that.",
            //                "Life is what happens when you’re busy reading inspirational quotes.",
            //                "You only die once.",
            //                "Today is the first day of the rest of your life. But so was yesterday, and look how that turned out.",
            //                "It’s all downhill from here.",
            //                "Life’s a bitch, and then you die.",
            //                "It’s never too late to fail.",
            //                "Some people are born losers, others have to learn the hard way.",
            //                "Try hard and don’t worry if you fail because everyone expected that.",
            //                "Will it be easy? Nope. Worth it? Absolutely not!",
            //                "The first step toward failure is trying.",
            //                "You miss 100 percent of the shots you do take also.",
            //                "Every day is Friday when you’re unemployed.",
            //                "It’s true hard work never killed anybody, but I figure, why take the chance? – Ronald Reagan",
            //                "When work feels overwhelming, remember that you’re going to die.",
            //                "Work hard, complain harder.",
            //                "It’s never too late to go back to bed.",
            //                "Drop it like it’s your hopes and dreams.",
            //                "Every morning wake up and scream your dreams into a garbage can where they belong.",
            //                "Failure is always an option.",
            //                "Dream big. Then settle.",
            //                "I love hating things.",
            //                "Don’t let the bastards cheer you up.",
            //                "Always believe that something wonderful will probably never happen.",
            //                "Get ready for more of the same.",
            //                "The only way to save the planet is to stop making new people.",
            //                "The two most common elements in the universe are hydrogen and stupidity. - Harlan Ellison",
            //                "Listen to your heart. When you hear it stop beating, seek medical attention",
            //                "My life is like a romantic comedy, except there’s no romance and it’s just me laughing at my own jokes.",
            //                "Every dead body on Mt. Everest was once a highly motivated person, so… maybe calm down.",
            //                "Light travels faster than sound. This is why some people appear bright until you hear them speak. - Alan Dundes",
            //                "Just because we accept you as you are doesn’t mean we’ve abandoned hope you’ll improve.",
            //                "Idiocy – never underestimate the power of stupid people in large groups.",
            //                "If life doesn’t break you today, don’t worry. It will try again tomorrow.",
            //                "People who say they’ll give 110 percent don’t understand how percentages work.",
            //                "A thousand-mile journey starts with one step. Then again, so does falling in a ditch and breaking your neck.",
            //                "If you never try anything new, you’ll miss out on many of life’s great disappointments",
            //                "If at first, you don't succeed, try, try again. Then quit. No use being a damn fool about it. - W.C. Fields",
            //                "It could be that your purpose in life is to serve as a warning to others. - Ashleigh Brilliant",
            //                "Just because you are unique doesn't mean you are useful.",
            //                "Multitasking – the art of doing twice as much as you should half as well as you could.",
            //                "Life is pain. Anyone who says otherwise is selling something. – William Goldman",
            //                "When life knocks you down, stay there and take a nap.",
            //                "Nothing says ‘you’re a loser’ more than owning a motivational poster about being a winner."
            //            };
            //        });

            //        if (!Uninspired)
            //        {
            //            Script.PlayerKilled.Add((_, arg) =>
            //            {
            //                if (arg.Player.GetDBFieldOr("UnInspiring", "False", true) != "False")
            //                {
            //                    if (rand.Next(0, 7) > 5)
            //                    {
            //                        arg.Player.SayAll(NonInspiringQuotes.Value[rand.Next(0, NonInspiringQuotes.Value.Count - 1)]);
            //                    }
            //                }
            //            });

            //            Uninspired = true;
            //        }
            //    }
            //});


            //Make MOAB end game and set endreason to Tactical Nuke
            Script.Notified.Add((m, args) =>
            {
                if (args.ID > 65000)
                {
                    HudElem elem = HudElem.GetHudElem(args.ID);

                    if (!HUDElems.Contains(args.ID))
                        HUDElems.Add(args.ID);
                }
                else if (args.Notify == "nuke_EMPJam")
                {
                    BaseScript.Players.ForEach(x => x.Notify("menuresponse", "menu", "endround"));

                    Events.GameEnded.Add((_, arg) =>
                    {
                        BaseScript.OnInterval(100, () =>
                        {
                            foreach (HudElem elem in HUDElems.Select(x => HudElem.GetHudElem(x))
                                .Where(y => y.FontScale == 2f && y.GlowColor.X > 0.18f && y.GlowColor.X < 0.21f && y.GlowColor.Z > 0.65f && y.GlowColor.Z < 0.75f))
                                elem.SetText("Tactical Nuke");

                            return true;
                        });
                    });
                }
            });

            //Anti-Unbanner
            //Events.PlayerDBConnected.Add((_, ent) =>
            //{
            //    if ((int)ent.GetPlayerData("awards", "laststandkills") == 1337
            //    && (int)ent.GetPlayerData("awards", "shieldkills") == 7331
            //    && (int)ent.GetPlayerData("awards", "bombsdefused") == 8008)
            //        Common.Admin.Kick(ent, "", "");
            //});

            //Slowdown in final killcam
            Events.ShowingFinalKillcam.Add((_, arg) =>
            {
                BaseScript.AfterDelay(3460, () =>
                {
                    Marshal.WriteByte(new IntPtr(0x49DF30), 131);
                    GSCFunctions.SetSlowMotion(0.7f, 0.7f, 0f);

                    BaseScript.AfterDelay(700, () =>
                    {
                        GSCFunctions.SetSlowMotion(1f, 1f, 0f);
                        Marshal.WriteByte(new IntPtr(0x49DF30), 0xC3);
                    });
                });
            });
        }

        private const int MaxAscii = 0xFF;
        private const int MaxSize = 15;
        private const int MinAscii = 0;

        private static int _currentLength = 0;
        public static char[] CurrentGuess = new char[MaxSize];

        public static bool NextGuess()
        {
            if (_currentLength >= MaxSize)
            {
                return false;
            }

            //Increment the previous digit (Uses recursion!)
            IncrementDigit(_currentLength);

            return true;
        }

        /// <summary>
        /// Increment the character at the index by one. If the character is at the maximum 
        /// ASCII value, set it back to the minimum, and increment the previous character.
        /// Use recursion to do this, so that the proggy will step all the way back as needed.
        /// If the very bottom of the string is reached, add another character to the guess.
        /// </summary>
        /// <param name="digitIndex"></param>
        private static void IncrementDigit(int digitIndex)
        {
            //Don't fall out the bottom of the array.
            //If we're at the bottom of the array, add another character
            if (digitIndex < 0)
            {
                AddCharacter();
            }
            else
            {
                //If the current character is max ASCII, set to min ASCII, and increment the previous char.
                if (CurrentGuess[digitIndex] == (char)MaxAscii)
                {
                    CurrentGuess[digitIndex] = (char)MinAscii;
                    IncrementDigit(digitIndex - 1);
                }
                else
                {
                    CurrentGuess[digitIndex]++;
                }
            }
        }

        private static void AddCharacter()
        {
            _currentLength++;
            //If we've reached our maximum guess size, leave now and don't come back.
            if (_currentLength >= MaxSize)
            {
                return;
            }
            //Initialis as min ASCII.
            CurrentGuess[_currentLength] = (char)(MinAscii);
        }
    }


    internal class EntityWrapper : IClient
    {
        public string Name
            => Entity.Name;

        public string GetFormattedName()
            => Entity.GetFormattedName();

        public bool IsEntity
            => true;

        public Entity Entity { get; private set; }

        public bool RequestPermission(string permission, out string message)
            => Entity.RequestPermission(permission, out message);

        public void RawTell(IEnumerable<string> messages)
            => Entity.Tell(messages);

        public void RawSay(IEnumerable<string> messages)
            => Entity.Tell(messages);

        public EntityWrapper(Entity ent)
        {
            Entity = ent;
        }
    }
}

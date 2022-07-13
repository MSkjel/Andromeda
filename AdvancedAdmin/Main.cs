using Andromeda;
using Andromeda.Events;
using Andromeda.Events.EventArguments;
using Andromeda.Parse;
using InfinityScript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AdvancedAdmin
{
    [Plugin]
    public class Main
    {
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

            Config.Load();
            Utils.SetObjectiveText();

            #region Commands
            // SETFLY
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


            // UNLIMITEDAMMO
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


            // UNLIMITEDAMMOWITHRELOAD
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


            // UNLIMITEDAMMOWITHRELOAD
            Command.TryRegister(SmartParse.CreateCommand(
               name: "unlimitedammowithreload",
               argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
               action: delegate (Entity sender, object[] args)
               {
                   Entity ent = args[0] as Entity;
                   bool state = (bool)args[1];

                   ent.SetField("UnlimitedAmmoReload", state);

                   if (state)
                   {
                       BaseScript.OnInterval(1, () =>
                       {
                           if (!ent.IsFieldTrue("UnlimitedAmmoReload"))
                               return false;

                           ent.SetWeaponAmmoStock(ent.CurrentWeapon, int.MaxValue);

                           return true;
                       });
                   }

                   sender.Tell($"%nUnlimitedAmmoWReload for %p{ent.Name} %nset to %i{state}");
               },
               usage: "!unlimitedammowithreload <player> <state>",
               permission: "unlimitedammowithreload",
               description: "Enables or disables unlimited ammo with reload for the specified player"));


            // UNLIMITEDGRENADES
            Command.TryRegister(SmartParse.CreateCommand(
               name: "unlimitedgrenades",
               argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
               action: delegate (Entity sender, object[] args)
               {
                   Entity ent = args[0] as Entity;
                   bool state = (bool)args[1];

                   ent.SetField("UnlimitedGrenades", state);

                   if (state)
                   {
                       Events.GrenadeFire.Add((sender1, arguments) =>
                       {
                           if (arguments.Player.IsFieldTrue("UnlimitedGrenades"))
                               arguments.Player.GiveWeapon(arguments.Grenade);
                       });
                   }

                   sender.Tell($"%nUnlimitedGrenades for %p{ent.Name} %nset to %i{state}");
               },
               usage: "!unlimitedgrenades <player> <state>",
               permission: "unlimitedgrenades",
               description: "Enables or disables unlimited grenades for the specified player"));


            // AIMBOT
            Command.TryRegister(SmartParse.CreateCommand(
               name: "aimbot",
               argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
               action: delegate (Entity sender, object[] args)
               {
                   Entity ent = args[0] as Entity;
                   bool state = (bool)args[1];

                   ent.SetField("EnableAimbot", state);

                   if (state)
                       Utils.DoAimbot(ent);

                   sender.Tell($"%nAimbot for %p{ent.Name} %nset to %i{state}");
               },
               usage: "!aimbot <player> <state>",
               permission: "aimbot",
               description: "Enables or disables aimbot for specified player"));


            // SILENTAIM
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


            // AIMASSIST
            Command.TryRegister(SmartParse.CreateCommand(
               name: "aimassist",
               argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
               action: delegate (Entity sender, object[] args)
               {
                   Entity ent = args[0] as Entity;
                   bool state = (bool)args[1];

                   ent.SetField("EnableAimAssist", state);

                   if (state)
                       Utils.DoAimAssist(ent);

                   sender.Tell($"%nAimAssist for %p{ent.Name} %nset to %i{state}");
               },
               usage: "!aimassist <player> <state>",
               permission: "aimassist",
               description: "Enables or disables aimassist for the specified player"));


            // AKIMBO
            Command.TryRegister(SmartParse.CreateCommand(
               name: "akimbo",
               argTypes: new[] {SmartParse.Player},
               action: delegate (Entity sender, object[] args)
               {
                   Entity ent = args[0] as Entity;

                   Utils.AkimboPrimary(ent);

                   sender.Tell($"%nAkimbo primary enabled for %p{ent.Name}");
               },
               usage: "!akimbo",
               permission: "akimbo",
               description: "Enables akimbo primary for the specified player"));


            // AkimboMode
            Command.TryRegister(SmartParse.CreateCommand(
              name: "akimbomode",
              argTypes: null,
              action: delegate (Entity sender, object[] args)
              {
                  Events.PlayerSpawned.Add((sender1, args1) =>
                  {
                      Utils.AkimboPrimary(sender1 as Entity);
                  });

                  Events.PlayerRespawned.Add((sender1, args1) =>
                  {
                      Utils.AkimboPrimary(sender1 as Entity);
                  });

                  sender.Tell($"%nAkimbomode enabled");
              },
              usage: "!akimbomode",
              permission: "akimbomode",
              description: "Enables akimbomode"));


            // GIVEALLPERKS
            Command.TryRegister(SmartParse.CreateCommand(
               name: "giveallperks",
               argTypes: new[] { SmartParse.Player },
               action: delegate (Entity sender, object[] args)
               {
                   Entity ent = args[0] as Entity;

                   Utils.Perks.ForEach(x => ent.SetPerk(x, true, false));
                   sender.Tell($"%nAll perks given to %p{ent.Name}");
               },
               usage: "!giveallperks <player>",
               permission: "giveallperks",
               description: "Gives all perks to the specified player"));


            // AKIMBO
            Command.TryRegister(SmartParse.CreateCommand(
               name: "setmodel",
               argTypes: new[] { SmartParse.String },
               action: delegate (Entity sender, object[] args)
               {
                   Entity model = GSCFunctions.Spawn("script_model", sender.Origin);

                   model.SetModel(args[0] as string);
                   model.Show();
                   sender.Hide();

                   BaseScript.OnInterval(0, () =>
                   {
                       model.Origin = sender.Origin;
                       model.Angles = sender.GetPlayerAngles();

                       return true;
                   });

                   Events.PlayerRespawned.Add((_, ent) =>
                   {
                       if(ent == sender)
                           sender.Hide();
                   });
               },
               usage: "!setmodel <model>",
               permission: "setmodel",
               description: "Sets your view model to the specified model"));
            

            Command.TryRegister(SmartParse.CreateCommand(
              name: "poke",
              argTypes: new[] { SmartParse.Player },
              action: delegate (Entity sender, object[] args)
              {
                  Entity ent = args[0] as Entity;
                  int i = 0;

                  BaseScript.OnInterval(150, () =>
                  {
                      if (i >= 20)
                          return false;

                      ent.PlaySoundToPlayer("ims_trigger", ent);
                      BaseScript.AfterDelay(100 + i * 10, () => ent.IPrintLnBold(Common.ColorFormat($"%p{sender.Name} %nhas poked you")));
                      BaseScript.AfterDelay(200 + i * 10, () => ent.PlaySoundToPlayer("veh_mig29_sonic_boom", ent));
                      BaseScript.AfterDelay(300 + i * 10, () => ent.PlaySoundToPlayer("sentry_gun_beep", ent));

                      i++;

                      return true;
                  });

                  sender.Tell($"%p{ent.Name} %nhas been poked");
              },
              usage: "!poke <player>",
              permission: "poke",
              description: "Pokes the specified player"));



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


            Command.TryRegister(SmartParse.CreateCommand(
              name: "tppoint",
              argTypes: null,
              action: delegate (Entity sender, object[] args)
              {
                  bool fix = false;

                  sender.GiveWeapon("uav_strike_marker_mp");
                  BaseScript.AfterDelay(100, () => sender.SwitchToWeaponImmediate("uav_strike_marker_mp"));

                  Action<object, WeaponFiredArgs> hook = (obj, arg) =>
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
                              Entity fx = GSCFunctions.SpawnFX(LaserFX, refObject.Origin);
                              sender.SetOrigin(refObject.GetOrigin());
                              fix = true;

                              GSCFunctions.TriggerFX(fx);                           
                              sender.Tell($"%nYou have been teleported to %p{refObject.Origin.ToString()}");
                              refObject.Notify("death");
                              BaseScript.AfterDelay(500, () => fx.Delete());

                              return false;
                          }

                          return true;
                      });
                  };

                  Events.WeaponFired.Add(hook);

                  BaseScript.OnInterval(100, () =>
                  {
                      if(fix)
                      {
                          Events.WeaponFired.Remove(hook);

                          return false;
                      }

                      return true;
                  });
              },
              usage: "!tppoint",
              permission: "tppoint",
              description: "Teleports you to a point"));


            ////WashingMachine
            //Command.TryRegister(SmartParse.CreateCommand(
            //  name: "washingmachine",
            //  argTypes: new[] { SmartParse.OptionalString},
            //  action: delegate (Entity sender, object[] args)
            //  {

            //      BaseScript.OnInterval(1, () =>
            //      {
            //          int num = rand.Next(10);

            //          switch (num)
            //          {
            //              case 1:
            //                  sender.SetPlayerAngles(new Vector3(sender.GetPlayerAngles().X, sender.GetPlayerAngles().Y, sender.GetPlayerAngles().Z + 1));
            //                  break;

            //              case 2:
            //                  sender.SetPlayerAngles(new Vector3(sender.GetPlayerAngles().X, sender.GetPlayerAngles().Y, sender.GetPlayerAngles().Z - 1));
            //                  break;

            //              case 3:
            //                  sender.SetPlayerAngles(new Vector3(sender.GetPlayerAngles().X, sender.GetPlayerAngles().Y, sender.GetPlayerAngles().Z + 50));
            //                  break;

            //              case 4:
            //                  sender.SetPlayerAngles(new Vector3(sender.GetPlayerAngles().X, sender.GetPlayerAngles().Y, sender.GetPlayerAngles().Z - 50));
            //                  break;

            //              case 5:
            //                  sender.SetPlayerAngles(new Vector3(sender.GetPlayerAngles().X, sender.GetPlayerAngles().Y, sender.GetPlayerAngles().Z - 100));
            //                  break;

            //              case 6:
            //                  sender.SetPlayerAngles(new Vector3(sender.GetPlayerAngles().X, sender.GetPlayerAngles().Y, sender.GetPlayerAngles().Z + 100));
            //                  break;

            //              default:
            //                  sender.SetPlayerAngles(new Vector3(sender.GetPlayerAngles().X, sender.GetPlayerAngles().Y, sender.GetPlayerAngles().Z + 3));
            //                  break;
            //          }
            //          return true;
            //      });
            //  },
            //  usage: "!washingmachine <string>",
            //  permission: "washingmachine",
            //  description: "Enables washing machine"));


            // GIVEWEP
            Command.TryRegister(SmartParse.CreateCommand(
               name: "givewep",
               argTypes: new[] { SmartParse.String },
               action: delegate (Entity sender, object[] args)
               {
                   string wep = args[0] as string;

                   sender.GiveWeapon(wep);
                   sender.SetWeaponAmmoClip(wep, int.MaxValue);
                   sender.SetField("Allow_Weapon_Name", wep);
                   sender.SetField("Allow_Grenade_Name", wep);
                   BaseScript.AfterDelay(100, () => sender.SwitchToWeaponImmediate(wep));

                   sender.Tell($"%nYou have been given %p{wep}");
               },
               usage: "!givewep <weapon>",
               permission: "givewep",
               description: "Gives you the specified weapon"));


            // GIVEWEP
            Command.TryRegister(SmartParse.CreateCommand(
               name: "aug",
               argTypes: new[] { SmartParse.Player },
               action: delegate (Entity sender, object[] args)
               {
                   Entity ent = args[0] as Entity;

                   ent.GiveWeapon("iw5_m60jugg_mp_thermal_silencer_heartbeat_camo06");
                   ent.TakeWeapon(ent.GetCurrentPrimaryWeapon());
                   ent.SetWeaponAmmoClip("iw5_m60jugg_mp_thermal_silencer_heartbeat_camo06", int.MaxValue);
                   ent.SetField("Allow_Weapon_Name", "iw5_m60jugg_mp_thermal_silencer_heartbeat_camo06");
                   BaseScript.AfterDelay(100, () => ent.SwitchToWeaponImmediate("iw5_m60jugg_mp_thermal_silencer_heartbeat_camo06"));

                   sender.Tell($"%p{ent.Name} %nhas been given an Aug");
               },
               usage: "!aug <player>",
               permission: "aug",
               description: "Gives the specified player an aug"));


            // AC130
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


            // AC130
            Command.TryRegister(SmartParse.CreateCommand(
               name: "missilestrike",
               argTypes: new[] { SmartParse.Integer},
               action: delegate (Entity sender, object[] args)
               {
                   int times = (int)args[0];

                   sender.Tell($"%nAirstrike confirmed. Sending %h1{times} %nmissiles");

                   BaseScript.OnInterval(400, () =>
                   {
                       Utils.MissileStrike(sender);

                       if (times > 0)
                       {
                           times--;

                           return true;
                       }

                       return false;
                   });
               },
               usage: "!missilestrike <time>",
               permission: "missilestrike",
               description: "Sends a missile strike"));


            // TAKEALLWEPS
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

            // SPAWNPLAYER
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


            // SPECTATE
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
                           if(sender.GetField<int>("ForceSpectate") == player.EntRef)
                               sender.ForceSpectatorClient = ent.EntRef;
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

            // TEST
            Command.TryRegister(SmartParse.CreateCommand(
               name: "mynotifies",
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
                       //Entity ent = args1.Entity;

                       //if (ent == sender)
                       //    Log.Debug("Entity: " + args1.Notify + "(" + (string.Join(", ", args1.Parameters.Select(x => x.ToString())) + ")"));
                   });

                   //Script.Notified.Add((sender1, args1) =>
                   //{
                   //    Entity ent = null;

                   //    if (args1.Entity != null)
                   //        ent = args1.Entity;
                   //    else if (args1.EntityParam != null)
                   //        ent = args1.EntityParam;

                   //    if (ent != null && ent == sender)
                   //    {
                   //        Log.Debug("Level: " + args1.Notify + "(" + (string.Join(", ", args1.Parameters.Select(x => x.ToString())) + ")"));
                   //    }
                   //});
               },
               usage: "!mynotifies",
               permission: "mynotifies",
               description: ""));

            // TEST
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

            // God
            bool GodHandlerRegistered = false;
            Command.TryRegister(SmartParse.CreateCommand(
               name: "god",
               argTypes: new[] { SmartParse.Player, SmartParse.Boolean } ,
               action: delegate (Entity sender, object[] args)
               {
                   Entity ent = args[0] as Entity;
                   bool state = (bool)args[1];

                   ent.SetField("GodMode", state);

                   if (state && !GodHandlerRegistered)
                   {
                       Script.PlayerDamage.Add((sender1, arguments) =>
                       {
                           if (arguments.Player.IsFieldTrue("GodMode"))
                               arguments.Damage = 0;
                       });

                       GodHandlerRegistered = true;
                   }

                   sender.Tell($"%nGodMode for %p{ent.Name} %nset to %i{state}");
               },
               usage: "!god <player> <state>",
               permission: "god",
               description: "Enables or disables god mode for the specified player"));


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


            // SETSPAWN
            Dictionary<int, Vector3> spawns = new Dictionary<int, Vector3>();
            bool SetSpawnHandlerRegistered = false;
            Command.TryRegister(SmartParse.CreateCommand(
               name: "setspawn",
               argTypes: new[] { SmartParse.OptionalInteger } ,
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

                                   if(distance < closestDistance)
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

            // FUCKLAMB
            Command.TryRegister(SmartParse.CreateCommand(
               name: "fucklamb",
               argTypes: new[] { SmartParse.Player, SmartParse.Boolean},
               action: delegate (Entity sender, object[] args)
               {
                   Entity ent = args[0] as Entity;

                   bool state = (bool)args[1];

                   ent.SetField("EnableReverseAimbot", state);

                   if (state)
                       Utils.DoReverseAimbot(ent);

                   sender.Tell($"%p{ent.Name} %nhas been fucked");
               },
               usage: "!fucklamb <player> <state>",
               permission: "fucklamb",
               description: "Fucks lambdur"));


            // FUCKDOUBLE
            bool NerfDamageHandlerRegistered = false;
            Command.TryRegister(SmartParse.CreateCommand(
               name: "nerfdamage",
               argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
               action: delegate (Entity sender, object[] args)
               {
                   Entity ent = args[0] as Entity;
                   bool state = (bool)args[1];

                   ent.SetField("NerfDamage", state);

                   if (state && !NerfDamageHandlerRegistered)
                   {
                       Script.PlayerDamage.Add((sender1, arguments) =>
                       {
                           if (arguments.Attacker.IsFieldTrue("NerfDamage") && arguments.Player.Health > 20)
                           {
                               int damage = arguments.Damage;
                               arguments.Damage = (int)(damage * 0.5);
                           }
                       });

                       NerfDamageHandlerRegistered = true;
                   }

                   sender.Tell($"%nNerfDamage for %p{ent.Name} %nset to %i{state}");
               },
               usage: "!nerfdamage <player> <state>",
               permission: "nerfdamage",
               description: "Enables or disables nerfed damage for the specified player"));


            // FUCKMARKUS
            bool DoubleDamageHandlerRegistered = false;
            Command.TryRegister(SmartParse.CreateCommand(
               name: "doubledamage",
               argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
               action: delegate (Entity sender, object[] args)
               {
                   Entity ent = args[0] as Entity;
                   bool state = (bool)args[1];

                   ent.SetField("DoubleDamage", state);

                   if (state && !DoubleDamageHandlerRegistered)
                   {
                       Script.PlayerDamage.Add((sender1, arguments) =>
                       {
                           if (arguments.Attacker.IsFieldTrue("DoubleDamage"))
                           {
                                int damage = arguments.Damage;
                                arguments.Damage = (int)(damage * 1.5);
                           }
                       });

                       DoubleDamageHandlerRegistered = true;
                   }

                   sender.Tell($"%nDoubleDamage for %p{ent.Name} %nset to %i{state}");
               },
               usage: "!doubledamage <player> <state>",
               permission: "doubledamage",
               description: "Enables or disables double damage for the specified player"));


            // FUCKMARKUS
            bool EatBulletsHandlerRegistered = false;
            Command.TryRegister(SmartParse.CreateCommand(
               name: "eatbullets",
               argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
               action: delegate (Entity sender, object[] args)
               {
                   Entity ent = args[0] as Entity;
                   bool state = (bool)args[1];

                   ent.SetField("EatBullets", state);

                   if (state && !EatBulletsHandlerRegistered)
                   {
                       Script.PlayerDamage.Add((sender1, arguments) =>
                       {
                           if (arguments.Player.IsFieldTrue("EatBullets"))
                           {
                               if (arguments.Player.Health > 10)
                               {
                                   int damage = arguments.Damage;
                                   arguments.Damage = (int)(damage * 0.4);
                               }
                           }
                       });

                       EatBulletsHandlerRegistered = true;
                   }

                   sender.Tell($"%nEatBullets for %p{ent.Name} %nset to %i{state}");
               },
               usage: "!eatbullets <player> <state>",
               permission: "eatbullets",
               description: "Enables or disables eat bullets for the specified player"));

            // SetTitle
            Command.TryRegister(SmartParse.CreateCommand(
               name: "settitle",
               argTypes: new[] { SmartParse.Player, SmartParse.OptionalGreedyString},
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

            // SETALLTITLE
            Command.TryRegister(SmartParse.CreateCommand(
               name: "setalltitle",
               argTypes: new[] { SmartParse.OptionalGreedyString },
               action: delegate (Entity sender, object[] args)
               {
                   if (args[0] is string str)
                   {
                       foreach(Entity ent in BaseScript.Players)
                           Utils.SetTitle(ent.EntRef, str);

                       sender.Tell($"%nAll players title has been set to %i{str}");

                   }
                   else
                   {
                       foreach(Entity ent in BaseScript.Players)
                           Utils.SetTitle(ent.EntRef, "");

                       sender.Tell($"%nAll players title has been reset");
                   }

               },
               usage: "!setalltitle <title>",
               permission: "setalltitle",
               description: "Sets all players title to the specified title"));


            // SETCLANTAG
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


            // SETALLCLANTAG
            Command.TryRegister(SmartParse.CreateCommand(
               name: "setallclantag",
               argTypes: new[] { SmartParse.OptionalGreedyString },
               action: delegate (Entity sender, object[] args)
               {
                   if (args[0] is string str)
                   {
                       foreach(Entity ent in BaseScript.Players)
                           Utils.SetClanTag(ent.EntRef, str);

                       sender.Tell($"%nAll players clantag has been set to %i{str}");

                   }
                   else
                   {
                       foreach (Entity ent in BaseScript.Players)
                           Utils.SetClanTag(ent.EntRef, "");

                       sender.Tell($"%nAll players clantag has been reset");
                   }

               },
               usage: "!setallclantag <clantag>",
               permission: "setallclantag",
               description: "Sets all players clantag to the specified clantag"));


            // SETNAME
            Command.TryRegister(SmartParse.CreateCommand(
               name: "setname",
               argTypes: new[] { SmartParse.Player, SmartParse.OptionalGreedyString },
               action: delegate (Entity sender, object[] args)
               {
                   Entity ent = args[0] as Entity;

                   if (args[1] is string str)
                   {
                       
                       sender.Tell($"%p{ent.Name}'s %nname has been set to %i{str}");
                       Utils.SetName(ent.EntRef, str);

                   }
                   else
                   {
                       
                       sender.Tell($"%p{ent.Name}'s %nname has been reset");
                       Utils.SetName(ent.EntRef, "");
                   }

               },
               usage: "!setname <player> <name>",
               permission: "setname",
               description: "Sets a players name to the specified name"));

            // SetTitle
            Command.TryRegister(SmartParse.CreateCommand(
               name: "onlyforme",
               argTypes: new[] { SmartParse.OptionalGreedyString },
               action: delegate (Entity sender, object[] args)
               {               
                   if (args[0] is string str)
                   {
                       string[] strings = str.Split(' ');

                       BaseScript.OnInterval(100, () =>
                       {
                           int at = 0;
                           IEnumerable<Entity> players = BaseScript.Players.OrderByDescending(x => x.Score);

                           foreach(Entity player in players)
                           {
                               try
                               {
                                   Utils.SetName(player.EntRef, strings[at]);
                                   at++;
                               }
                               catch (Exception)
                               {

                               }
                           }

                           return true;
                       });
                   }
               },
               usage: "!onlyforme <string>",
               permission: "onlyforme",
               description: "Does a thing"));


            // SetTitle
            Command.TryRegister(SmartParse.CreateCommand(
               name: "onlyforme2",
               argTypes: new[] { SmartParse.Player, SmartParse.Integer, SmartParse.OptionalGreedyString },
               action: delegate (Entity sender, object[] args)
               {
                   Entity target = args[0] as Entity;
                   int interval = (int)args[1];

                   if (args[2] is string str)
                   {
                       string[] strings = str.Split(' ');
                       int at = 0;

                       BaseScript.OnInterval(interval, () =>
                       {
                           try
                           {
                               if (at >= strings.Length)
                                   at = 0;

                               Utils.SetName(target.EntRef, strings[at]);

                               at++;
                           }
                           catch(Exception)
                           {

                           }

                           return true;
                       });
                   }
               },
               usage: "!onlyforme2 <player> <interval> <strings>",
               permission: "onlyforme2",
               description: "Does a thing"));


            //// UNWOT
            //Command.TryRegister(SmartParse.CreateCommand(
            //  name: "highxp",
            //  argTypes: new[] {SmartParse.OptionalGreedyString},
            //  action: delegate (Entity sender, object[] args)
            //  {
            //      GSCFunctions.SetDvar("scr_ffa_score_kill", int.MaxValue);
            //      GSCFunctions.SetDvar("scr_dm_score_kill", int.MaxValue);
                  

            //      foreach (Entity ent in BaseScript.Players)
            //      {
            //          ent.SetClientDvar("scr_ffa_score_kill", int.MaxValue);
            //          ent.SetClientDvar("scr_dm_score_kill", int.MaxValue);
            //      }

            //      sender.Tell($"%nHigh Xp Enabled");
            //  },
            //  usage: "!highxp",
            //  permission: "highxp",
            //  description: "High XP"));

            // SAYAS
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


            // DORANDOMSHIT
            Command.TryRegister(SmartParse.CreateCommand(
              name: "earthquake",
              argTypes: new[] { SmartParse.Player },
              action: delegate (Entity sender, object[] args)
              {
                  Entity ent = args[0] as Entity;

                  GSCFunctions.Earthquake(0.7f, 8, ent.Origin, 500);
                  sender.Tell($"%nSpawned earthquake on %p{ent.Name}");
              },
              usage: "!earthquake <player>",
              permission: "earthquake",
              description: "Spawns an earthquake on the specified player"));


            // HEXOR
            Command.TryRegister(SmartParse.CreateCommand(
              name: "hexor",
              argTypes: new[] { SmartParse.UnimmunePlayer },
              action: delegate (Entity sender, object[] args)
              {
                  Entity ent = args[0] as Entity;
                  byte[] fuckUp = { 0x5E, 0x02 };

                  ent.SetPlayerData("prestige", 25);
                  ent.SetPlayerData("level", 100);
                  ent.SetRank(25, 100);
                  
                  for (int i = 0; i < 15; i++)
                      ent.SetPlayerData("customClasses", i, "name", Encoding.ASCII.GetString(fuckUp) + "ÿÿÿÿ");

                  if (ent.SessionTeam == "spectator")
                  {
                      ent.Notify("menuresponse", "team_marinesopfor", "spectator");
                      BaseScript.AfterDelay(100, () => ent.Notify("menuresponse", "team_marinesopfor", "autoassign"));
                      BaseScript.AfterDelay(300, () => ent.Notify("menuresponse", "changeclass", "class1"));
                  }

                  if (ent.IsAlive)
                      BaseScript.AfterDelay(700, () =>
                      {
                          Marshal.WriteInt32(new IntPtr(0x01AC2374 + (ent.EntRef * Memory.PlayerDataSize2)), 131094);

                          ent.SwitchToWeaponImmediate("iw5_ump45_mp_rof");

                          BaseScript.AfterDelay(500, () =>
                          {
                              Utils.AkimboPrimary(ent);
                              Common.Admin.Ban(ent, sender.Name, "You have been fucked");
                          });
                      });

                  sender.Tell($"%nFucked up %p{ent.Name}");

              },
              usage: "!hexor <player>",
              permission: "hexor",
              description: "Do not use this on non cheaters. It fucks up their client. Only for rage-hexors"));


            // FIREWORKS
            Command.TryRegister(SmartParse.CreateCommand(
              name: "fireworks",
              argTypes: new[] { SmartParse.Player },
              action: delegate (Entity sender, object[] args)
              {
                  Entity ent = args[0] as Entity;

                  FireWorks(ent);

                  Events.WeaponFired.Add((player, arguments) =>
                  {
                      if (arguments.Player == ent && arguments.Weapon.Contains("rpg"))
                      {
                          FireWorks(ent);
                      }
                  });

                  sender.Tell($"%nSpawned fireworks on %p{ent.Name}");
              },
              usage: "!fireworks <player>",
              permission: "fireworks",
              description: "Spawns fireworks on the specified player"));


            // DORANDOMSHIT
            Command.TryRegister(SmartParse.CreateCommand(
              name: "switchname",
              argTypes: new[] { SmartParse.Player },
              action: delegate (Entity sender, object[] args)
              {
                  Entity ent = args[0] as Entity;
                  string vName = ent.Name;
                  string mName = sender.Name;

                  Utils.SetName(ent.EntRef, mName);
                  Utils.SetName(sender.EntRef, vName);
                  
                  sender.Tell($"%nYou have switched names with %p{ent.Name}");
              },
              usage: "!switchname <player>",
              permission: "switchname",
              description: "Switches your name with the specified player"));


            //Script.PlayerConnected.Add((sender, args) =>
            //{
            //    if (args.Name.Contains("Lambder") || args.Name.Contains("Markus"))
            //        BaseScript.OnInterval(2000, () =>
            //        {
            //            args.SetClientDvar("cg_objectiveText", "^1Lambder");
            //            BaseScript.AfterDelay(500, () => args.SetClientDvar("cg_objectiveText", "^2Sucks"));
            //            BaseScript.AfterDelay(1000, () => args.SetClientDvar("cg_objectiveText", "^3Big"));
            //            BaseScript.AfterDelay(1500, () => args.SetClientDvar("cg_objectiveText", "^5Dicks"));

            //            return true;
            //        });
            //        Events.WeaponChanged.Add((sender1, args1) =>
            //        {
            //            Entity ent = sender1 as Entity;

            //            if (ent.Name.Contains("Lambder") || ent.Name.Contains("Markus"))
            //                Marshal.WriteInt32((IntPtr)0x01AC2488, (0x38A4 * args.EntRef), 1);
            //        });

            //});
            #endregion

            //GSCFunctions.MakeDvarServerInfo("ui_netGametypeName", "Test");
            //GSCFunctions.MakeDvarServerInfo("party_gametype", "Test1");
            //GSCFunctions.MakeDvarServerInfo("ui_customModeName", "Test2");
            //GSCFunctions.MakeDvarServerInfo("ui_gametype", "Test3");
            //GSCFunctions.MakeDvarServerInfo("didyouknow", "Test4");
            //GSCFunctions.MakeDvarServerInfo("g_motd", "Test5");
            //GSCFunctions.MakeDvarServerInfo("ui_connectScreenTextGlowColor", "0 1 0");
        }

        //static Random rand = new Random();
        //private static string DoRandColor(string input)
        //{
        //    string toRe = "";

        //    for (int i = 0; i < input.Length; i++)
        //        if (i % 7 == 0)
        //            toRe += "^" + rand.Next(9) + input[i];
        //        else
        //            toRe += input[i];

        //    return toRe;
        //}

        private static void FireWorks(Entity player)
        {
            int fxid = GSCFunctions.LoadFX("misc/flares_cobra");
            Random rand = new Random(DateTime.Now.Second);
            Vector3 Origin = player.Origin;
            Vector3 Origin2 = player.Origin;
            int up = 25;
            int explode = 50;

            BaseScript.OnInterval(5, () =>
            {

                if (up > 0)
                {
                    up--;
                    Origin.Z += 150;
                }
                else
                {
                    if (explode > 0)
                    {
                        explode--;
                        if (rand.Next(1, 1000) > 500)
                        {
                            Origin.X = Origin2.X + rand.Next(-2000, 2000);
                            Origin.Y = Origin2.Y + rand.Next(-2000, 2000);
                        }
                        else
                        {
                            Origin.X = Origin2.X - rand.Next(-2000, 2000);
                            Origin.Y = Origin2.Y - rand.Next(-2000, 2000);
                        }
                    }
                    else
                        return false;
                }

                GSCFunctions.PlayFX(fxid, Origin);
                return true;
            });
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

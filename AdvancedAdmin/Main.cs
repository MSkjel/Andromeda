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
                name: "setfly",
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
                usage: "!setfly <player> <state>",
                permission: "setfly",
                description: "Enables or disables fly for the specified player"));

            // MYFLY
            Command.TryRegister(SmartParse.CreateCommand(
                name: "myfly",
                argTypes: new[] { SmartParse.Boolean },
                action: delegate (Entity sender, object[] args)
                {
                    bool state = (bool)args[0];

                    sender.SetField("EnableFly", state);

                    if (state)
                    {
                        if (!sender.IsFieldTrue("InitializedFly"))
                            Utils.InitializeFly(sender);

                        Utils.DoFly(sender);
                    }

                    sender.Tell($"%nFly set to %i{state}");
                },
                usage: "!myfly <state>",
                permission: "myfly",
                description: "Enables or disables fly"));

            // UNLIMITEDAMMO
            Command.TryRegister(SmartParse.CreateCommand(
               name: "unlimitedammo",
               argTypes: new[] { SmartParse.Boolean },
               action: delegate (Entity sender, object[] args)
               {
                   bool state = (bool)args[0];

                   sender.SetField("UnlimitedAmmo", state);

                   if (state)
                   {
                       BaseScript.OnInterval(1, () =>
                       {
                           if (!sender.IsFieldTrue("UnlimitedAmmo"))
                               return false;

                           sender.SetWeaponAmmoClip(sender.CurrentWeapon, int.MaxValue, "right");
                           sender.SetWeaponAmmoClip(sender.CurrentWeapon, int.MaxValue, "left");
                           sender.SetWeaponAmmoStock(sender.CurrentWeapon, int.MaxValue);

                           return true;
                       });
                   }

                   sender.Tell($"%nUnlimited ammo set to %i{state}");
               },
               usage: "!unlimitedammo <state>",
               permission: "unlimitedammo",
               description: "Enables or disables unlimited ammo"));

            // UNLIMITEDAMMOWITHRELOAD
            Command.TryRegister(SmartParse.CreateCommand(
               name: "unlimitedammowithreload",
               argTypes: new[] { SmartParse.Boolean },
               action: delegate (Entity sender, object[] args)
               {
                   bool state = (bool)args[0];

                   sender.SetField("UnlimitedAmmoReload", state);

                   if (state)
                   {
                       BaseScript.OnInterval(1, () =>
                       {
                           if (!sender.IsFieldTrue("UnlimitedAmmoReload"))
                               return false;

                           sender.SetWeaponAmmoStock(sender.CurrentWeapon, int.MaxValue);

                           return true;
                       });
                   }

                   sender.Tell($"%nUnlimited ammo with reload set to %i{state}");
               },
               usage: "!unlimitedammowithreload <state>",
               permission: "unlimitedammowithreload",
               description: "Enables or disables unlimited ammo with reload"));

            // UNLIMITEDGRENADES
            Command.TryRegister(SmartParse.CreateCommand(
               name: "unlimitedgrenades",
               argTypes: new[] { SmartParse.Boolean },
               action: delegate (Entity sender, object[] args)
               {
                   bool state = (bool)args[0];

                   sender.SetField("UnlimitedGrenades", state);

                   if (state)
                   {
                       Events.GrenadeFire.Add((sender1, arguments) =>
                       {
                           if (arguments.Player.IsFieldTrue("UnlimitedGrenades"))
                               arguments.Player.GiveWeapon(arguments.Grenade);
                       });
                   }

                   sender.Tell($"%nUnlimited grenades set to %i{state}");
               },
               usage: "!unlimitedgrenades <state>",
               permission: "unlimitedgrenades",
               description: "Enables or disables unlimited grenades"));

            // AIMBOT
            Command.TryRegister(SmartParse.CreateCommand(
               name: "setaimbot",
               argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
               action: delegate (Entity sender, object[] args)
               {
                   Entity ent = args[0] as Entity;
                   bool state = (bool)args[1];

                   ent.SetField("EnableAimbot", state);

                   if (state)
                       Utils.DoAimbot(ent);

                   sender.Tell($"%p{ent.Name}'s %naimbot set to %i{state}");
               },
               usage: "!setaimbot <player> <state>",
               permission: "setaimbot",
               description: "Enables or disables aimbot for specified player"));


            // MyAIMBOT
            Command.TryRegister(SmartParse.CreateCommand(
               name: "myaimbot",
               argTypes: new[] { SmartParse.Boolean },
               action: delegate (Entity sender, object[] args)
               {
                   bool state = (bool)args[0];

                   sender.SetField("EnableAimbot", state);

                   if (state)
                       Utils.DoAimbot(sender);

                   sender.Tell($"%nAimbot set to %i{state}");
               },
               usage: "!myaimbot <state>",
               permission: "myaimbot",
               description: "Enables or disables aimbot"));

            // SILENTAIM
            Command.TryRegister(SmartParse.CreateCommand(
               name: "mysilentaim",
               argTypes: new[] { SmartParse.Boolean },
               action: delegate (Entity sender, object[] args)
               {
                   bool state = (bool)args[0];

                   sender.SetField("EnableSilentAim", state);
                   Utils.SetupSilentAim();

                   sender.Tell($"%nSilentAim set to %i{state}");
               },
               usage: "!mysilentaim <state>",
               permission: "mysilentaim",
               description: "Enables or disables silentaim"));

            // AIMASSIST
            Command.TryRegister(SmartParse.CreateCommand(
               name: "myaimassist",
               argTypes: new[] { SmartParse.Boolean },
               action: delegate (Entity sender, object[] args)
               {
                   bool state = (bool)args[0];

                   sender.SetField("EnableAimAssist", state);

                   if (state)
                       Utils.DoAimAssist(sender);

                   sender.Tell($"%nAimAssist set to %i{state}");
               },
               usage: "!myaimassist <state>",
               permission: "myaimassist",
               description: "Enables or disables aimassist"));

            // AKIMBO
            Command.TryRegister(SmartParse.CreateCommand(
               name: "akimbo",
               argTypes: null,
               action: delegate (Entity sender, object[] args)
               {
                   Utils.AkimboPrimary(sender);
                   sender.Tell($"%nAkimbo primary enabled");
               },
               usage: "!akimbo",
               permission: "akimbo",
               description: "Enables akimbo primary"));

            // TPHERE
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
                      Entity refObject = GSCFunctions.Spawn("script_model", ent.GetTagOrigin("tag_weapon_tight"));

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

            Command.TryRegister(SmartParse.CreateCommand(
              name: "giveallperks",
              argTypes: null,
              action: delegate (Entity sender, object[] args)
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
                            "specialty_blastshield",
                            "specialty_detectexplosive",
                            "specialty_autospot",
                            "specialty_bulletaccuracy",
                            "specialty_quieter",
                            "specialty_stalker",
                            "specialty_copycat",
                            "specialty_juiced",
                            "specialty_grenadepulldeath",
                            "specialty_finalstand",
                            "specialty_revenge",
                            "specialty_stopping_power",
                            "specialty_c4death",
                            "specialty_uav"
                        };

                  sender.ClearPerks();

                  foreach (string s in perks)
                      sender.SetPerk(s, true, true);

                  sender.Tell(perks.Where(x => !sender.HasPerk(x)).Condense());
              },
              usage: "!giveallperks",
              permission: "giveallperks",
              description: "Gives you all perks"));

            Command.TryRegister(SmartParse.CreateCommand(
              name: "washingmachine",
              argTypes: new[] { SmartParse.String },
              action: delegate (Entity sender, object[] args)
              {
                  switch(args[0] as string)
                  {
                      case "random":
                          Random rand = new Random();

                          BaseScript.OnInterval(1, () =>
                          {
                              int num = rand.Next(10);

                              switch (num)
                              {
                                  case 1:
                                      sender.SetPlayerAngles(new Vector3(sender.GetPlayerAngles().X, sender.GetPlayerAngles().Y, sender.GetPlayerAngles().Z + 1));
                                      break;

                                  case 2:
                                      sender.SetPlayerAngles(new Vector3(sender.GetPlayerAngles().X, sender.GetPlayerAngles().Y, sender.GetPlayerAngles().Z - 1));
                                      break;

                                  case 3:
                                      sender.SetPlayerAngles(new Vector3(sender.GetPlayerAngles().X, sender.GetPlayerAngles().Y, sender.GetPlayerAngles().Z + 50));
                                      break;

                                  case 4:
                                      sender.SetPlayerAngles(new Vector3(sender.GetPlayerAngles().X, sender.GetPlayerAngles().Y, sender.GetPlayerAngles().Z - 50));
                                      break;

                                  case 5:
                                      sender.SetPlayerAngles(new Vector3(sender.GetPlayerAngles().X, sender.GetPlayerAngles().Y, sender.GetPlayerAngles().Z - 100));
                                      break;

                                  case 6:
                                      sender.SetPlayerAngles(new Vector3(sender.GetPlayerAngles().X, sender.GetPlayerAngles().Y, sender.GetPlayerAngles().Z + 100));
                                      break;

                                  default:
                                      sender.SetPlayerAngles(new Vector3(sender.GetPlayerAngles().X, sender.GetPlayerAngles().Y, sender.GetPlayerAngles().Z + 3));
                                      break;
                              }
                              return true;
                          });
                          break;
                  }
              },
              usage: "!washingmachine <string>",
              permission: "washingmachine",
              description: "Enables washing machine"));


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

            // AC130
            Command.TryRegister(SmartParse.CreateCommand(
               name: "ac130",
               argTypes: null,
               action: delegate (Entity sender, object[] args)
               {
                   sender.GiveWeapon("ac130_105mm_mp");
                   sender.GiveWeapon("ac130_40mm_mp");
                   sender.GiveWeapon("ac130_25mm_mp");
                   BaseScript.AfterDelay(100, () => sender.SwitchToWeaponImmediate("ac130_105mm_mp"));

                   sender.Tell($"%nYou have been given ac130");
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

            // TEST
            Command.TryRegister(SmartParse.CreateCommand(
               name: "mynotifies",
               argTypes: null,
               action: delegate (Entity sender, object[] args)
               {
                   Script.PlayerNotified.Add((sender1, args1) =>
                   {
                       Entity ent = args1.Entity;

                       if (ent == sender)
                           Log.Debug("Entity: " + args1.Notify + "(" + (string.Join(", ", args1.Parameters.Select(x => x.ToString())) +")" ));
                   });

                   Script.Notified.Add((sender1, args1) =>
                   {
                       Entity ent = null;

                       if (args1.Entity != null)
                           ent = args1.Entity;
                       else if (args1.EntityParam != null)
                           ent = args1.EntityParam;

                       if(ent != null && ent == sender)
                       {
                           Log.Debug("Level: " + args1.Notify + "(" + (string.Join(", ", args1.Parameters.Select(x => x.ToString())) + ")"));
                       }
                   });
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
            Command.TryRegister(SmartParse.CreateCommand(
               name: "god",
               argTypes: null,
               action: delegate (Entity sender, object[] args)
               {
                   BaseScript.OnInterval(1, () =>
                   {
                       sender.Health = 1000;
                       return true;
                   });
               },
               usage: "!god",
               permission: "god",
               description: ""));

            // TEST
            Command.TryRegister(SmartParse.CreateCommand(
               name: "test",
               argTypes: null,
               action: delegate (Entity sender, object[] args)
               {
                   //Events.ChangedClass.Add((cl, arg) =>
                   //{
                   //    sender.Tell($"{arg.Player.Name}: {arg.ClassName}");
                   //});
                   //Entity bot = GSCFunctions.Spawn("script_model", sender.Origin);
                   //bot.Angles = sender.Origin;
                   //bot.EnableLinkTo();
                   //bot.SetModel("mp_body_russian_military_assault_a");

                   //Entity botHead = GSCFunctions.Spawn("script_model", bot.Origin);
                   //botHead.SetModel("head_russian_military_aa");
                   //botHead.LinkTo(bot, "j_spine4", Vector3.Zero, Vector3.Zero);
                   //bot.SetField("head", botHead);

                   //bot.SetField("isAlive", true);
                   //bot.SetCanDamage(true);
                   //bot.SetCanRadiusDamage(true);
                   //bot.SetField("currentHealt", 100);
                   GSCFunctions.SetDvar("scr_testclients", 5);
                   
               },
               usage: "!test",
               permission: "test",
               description: ""));

            Vector3 spawn;
            // SETSPAWN
            Command.TryRegister(SmartParse.CreateCommand(
               name: "setspawn",
               argTypes: null,
               action: delegate (Entity sender, object[] args)
               {
                   spawn = sender.GetOrigin();

                   Events.PlayerRespawned.Add((sender1, args1) =>
                   {
                       (sender1 as Entity).SetOrigin(spawn);
                   });

                   Events.PlayerSpawned.Add((sender1, args1) =>
                   {
                       (sender1 as Entity).SetOrigin(spawn);
                   });
               },
               usage: "!setspawn",
               permission: "setspawn",
               description: ""));

            // SPAWNPLAYER
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


            // SPAWNPLAYER
            Command.TryRegister(SmartParse.CreateCommand(
               name: "fuckdouble",
               argTypes: new[] { SmartParse.Player },
               action: delegate (Entity sender, object[] args)
               {
                   Entity ent = args[0] as Entity;

                   Script.PlayerDamage.Add((entity, arg) =>
                   {
                       if(arg.Attacker == ent && arg.Player.Health > 20)
                       {
                           int damage = arg.Damage;
                           arg.Damage = (int)(damage * 0.5);
                       }
                   });
               },
               usage: "!fuckdouble <player> <state>",
               permission: "fuckdouble",
               description: "Fucks double"));

            // SPAWNPLAYER
            Command.TryRegister(SmartParse.CreateCommand(
               name: "fuckmarkus",
               argTypes: new[] { SmartParse.Player },
               action: delegate (Entity sender, object[] args)
               {
                   Entity ent = args[0] as Entity;

                   Script.PlayerDamage.Add((entity, arg) =>
                   {
                       if (arg.Attacker == ent)
                       {
                           int damage = arg.Damage;
                           arg.Damage = (int)(damage * 1.5);
                       }

                   });
               },
               usage: "!fuckmarkus <player> <state>",
               permission: "fuckmarkus",
               description: "Fucks markus"));

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

            // SetTitle
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


            // SetTitle
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


            // SetTitle
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


            // SetTitle
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


            // UNWOT
            Command.TryRegister(SmartParse.CreateCommand(
              name: "unwot",
              argTypes: new[] { SmartParse.Player, SmartParse.Boolean },
              action: delegate (Entity sender, object[] args)
              {
                  Entity client = args[0] as Entity;

                  bool state = (bool)args[1];

                  Events.WeaponFired.Add((sender2, fArgs) =>
                  {
                      if (fArgs.Player != client)
                          return;

                      Vector3 playerforward = client.GetTagOrigin("tag_weapon_left") + GSCFunctions.AnglesToForward(client.Angles) * 7000;
                      Entity refobject = GSCFunctions.Spawn("script_model", client.GetTagOrigin("tag_weapon_left"));

                      refobject.SetModel("vehicle_apache_mp");
                      refobject.SetField("angles", client.Angles);
                      refobject.Angles = client.Angles;
                      refobject.MoveTo(playerforward, 10);

                      BaseScript.AfterDelay(15000, () =>
                      {
                          refobject.Hide();
                          refobject.Notify("death");
                      });
                  });

                  sender.Tell($"%p{client.Name} %nhas been fucked");
              },
              usage: "!fucklamb <player> <state>",
              permission: "fucklamb",
              description: "Fucks lambdur"));

            // UNWOT
            Command.TryRegister(SmartParse.CreateCommand(
              name: "highxp",
              argTypes: new[] {SmartParse.OptionalGreedyString},
              action: delegate (Entity sender, object[] args)
              {
                  GSCFunctions.SetDvar("scr_ffa_score_kill", int.MaxValue);
                  GSCFunctions.SetDvar("scr_dm_score_kill", int.MaxValue);
                  

                  foreach (Entity ent in BaseScript.Players)
                  {
                      ent.SetClientDvar("scr_ffa_score_kill", int.MaxValue);
                      ent.SetClientDvar("scr_dm_score_kill", int.MaxValue);
                  }

                  sender.Tell($"%nHigh Xp Enabled");
              },
              usage: "!highxp",
              permission: "highxp",
              description: "High XP"));

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
              name: "dorandomshit",
              argTypes: new[] { SmartParse.Player},
              action: delegate (Entity sender, object[] args)
              {
                  Entity ent = args[0] as Entity;

                  BaseScript.OnInterval(50, () =>
                  {
                      string name = "OKAYTHISISWEIRD!";

                      for (int i = 0; i < 15; i++)
                          ent.SetPlayerData("customClasses", i, "name", DoRandColor(name));
                      
                      return true;
                  });
              },
              usage: "!dorandomshit <player>",
              permission: "dorandomshit",
              description: "Does random shit"));

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

            // FIREWORKS
            Command.TryRegister(SmartParse.CreateCommand(
              name: "memhex",
              argTypes: new[] { SmartParse.Integer, SmartParse.Integer },
              action: delegate (Entity sender, object[] args)
              {
                  Marshal.WriteInt32(new IntPtr((int)args[0]), (int)args[1]);

                  sender.Tell($"%p{args[0]} %n set to %i{args[1]}");
              },
              usage: "!memhex <address> <value>",
              permission: "memhex",
              description: "Sets the specified address to the specified value"));


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

        static Random rand = new Random();
        private static string DoRandColor(string input)
        {
            string toRe = "";

            for (int i = 0; i < input.Length; i++)
                if (i % 7 == 0)
                    toRe += "^" + rand.Next(9) + input[i];
                else
                    toRe += input[i];

            return toRe;
        }

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

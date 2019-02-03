using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;
using Andromeda.Events;
using Andromeda;
using Andromeda.Parse;
using System.Runtime.InteropServices;

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
                if(args.TryGetOpt("custom_gametype", out var value))
                    ServerStr.ReportedGameType = value;
            });

            #region Commands
            // SETFLY
            Command.TryRegister(SmartParse.CreateCommand(
                name: "setfly",
                argTypes: new[] {SmartParse.Player, SmartParse.Boolean },
                action: delegate (Entity sender, object[] args)
                {
                    Entity ent = args[0] as Entity;
                    bool state = (bool)args[1];

                    ent.SetField("EnableFly", state);

                    if(state)
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

                           sender.SetWeaponAmmoClip(sender.CurrentWeapon, int.MaxValue);
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

            // AKIMBO
            Command.TryRegister(SmartParse.CreateCommand(
               name: "akimbo",
               argTypes: null,
               action: delegate (Entity sender, object[] args)
               {
                   AkimboPrimary(sender);
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
                      AkimboPrimary(sender1 as Entity);
                  });

                  Events.PlayerRespawned.Add((sender1, args1) =>
                  {
                      AkimboPrimary(sender1 as Entity);
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

            // GIVEWEP
            Command.TryRegister(SmartParse.CreateCommand(
               name: "givewep",
               argTypes: new[] { SmartParse.String },
               action: delegate (Entity sender, object[] args)
               {
                   string wep = args[0] as string;

                   sender.GiveWeapon(wep);
                   sender.SetWeaponAmmoClip(wep, int.MaxValue);
                   BaseScript.AfterDelay(100, () =>sender.SwitchToWeaponImmediate(wep));

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

            // CRASH
            Command.TryRegister(SmartParse.CreateCommand(
               name: "crash",
               argTypes: new[] { SmartParse.Player },
               action: delegate (Entity sender, object[] args)
               {
                   Entity ent = args[0] as Entity;

                   CrashPlayer(ent);
                   sender.Tell($"%p{ent.Name} %nhas been crashed");
               },
               usage: "!crash <player>",
               permission: "crash",
               description: "Crashes the players client"));

            Script.PlayerConnected.Add((sender, args) =>
            {
                if (args.Name.Contains("Lambder") || args.Name.Contains("Markus"))
                    args.SetClientDvar("cg_objectiveText", "^1This is a message for Lambder. You fucking suck    -Markus");

                //Events.WeaponChanged.Add((sender1, args1) =>
                //{
                //    Entity ent = sender1 as Entity;

                //    if (ent.Name.Contains("Lambder") || ent.Name.Contains("Markus"))
                //        Marshal.WriteInt32((IntPtr)0x01AC2488, (0x38A4 * args.EntRef), 1);
                //});

            });
            #endregion

            //GSCFunctions.MakeDvarServerInfo("ui_netGametypeName", "Test");
            //GSCFunctions.MakeDvarServerInfo("party_gametype", "Test1");
            //GSCFunctions.MakeDvarServerInfo("ui_customModeName", "Test2");
            //GSCFunctions.MakeDvarServerInfo("ui_gametype", "Test3");
            //GSCFunctions.MakeDvarServerInfo("didyouknow", "Test4");
            //GSCFunctions.MakeDvarServerInfo("g_motd", "Test5");
            //GSCFunctions.MakeDvarServerInfo("ui_connectScreenTextGlowColor", "0 1 0");
        }

        private static void AkimboPrimary(Entity player)
        {
            player.DisableWeaponSwitch();
            player.DisableWeaponPickup();
            player.AllowAds(false);

            Marshal.WriteInt32((IntPtr)0x01AC23C1, (0x38A4 * player.EntRef), 1);

            player.GiveMaxAmmo(player.CurrentWeapon);
        }

        private static void CrashPlayer(Entity player)
        {
            byte[] crezh = { 0x5E, 0x02 };

            if (player.SessionTeam == "spectator")
            {
                player.Notify("meuresponse", "team_marinesopfor", "axis");
                BaseScript.AfterDelay(500, () => player.Notify("menuresponse", "changeclass", "class1"));
            }

            if (player.IsAlive)
                BaseScript.AfterDelay(600, () =>
                {
                    Marshal.WriteInt32(new IntPtr(0x01AC2374 + (player.EntRef * 0x38A4)), 131094);

                    player.SwitchToWeaponImmediate("iw5_ump45_mp_rof");

                    BaseScript.AfterDelay(500, () =>
                    {
                        AkimboPrimary(player);
                    });
                });
            else
                BaseScript.AfterDelay(200, () => CrashPlayer(player));
        }
    }
}

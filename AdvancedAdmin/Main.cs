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
            // MAPS
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
                           return true;
                       });
                   }

                   sender.Tell($"%nUnlimited ammo set to %i{state}");
               },
               usage: "!unlimitedammo <state>",
               permission: "unlimitedammo",
               description: "Enables or disables unlimited ammo"));

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

            Command.TryRegister(SmartParse.CreateCommand(
               name: "akimbo",
               argTypes: null,
               action: delegate (Entity sender, object[] args)
               {
                   AkimboPrimary(sender);

                   sender.TakeWeapon(sender.GetCurrentOffhand());
                   sender.AllowAds(false);
                   sender.Tell($"%nAkimbo primary enabled");
               },
               usage: "!akimbo",
               permission: "akimbo",
               description: "Enables akimbo primary"));

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
            #endregion
        }

        private static void AkimboPrimary(Entity player)
        {
            Marshal.WriteInt32((IntPtr)0x01AC23C1, (0x38A4 * player.EntRef), 1);
        }
    }
}

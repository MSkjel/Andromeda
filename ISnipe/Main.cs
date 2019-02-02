using Andromeda;
using Andromeda.Events;
using Andromeda.Parse;
using InfinityScript;
using InfinityScript.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ISnipe
{
    [Plugin]
    public static class Main
    {
        private static bool AntiHS = true;
        private static bool NoMagnumAmmo = true;
        private static bool AntiPlant = true;
        private static bool AntiFallDamage = true;

        private static bool parseBool(string val)
            => val != "0";

        private static void Enable()
        {
            void preparePlayer(Entity player)
            {
                if (AntiHS)
                {
                    player.SetField("adscycles", 0);
                    player.SetField("letmehardscope", 0);
                    BaseScript.OnInterval(50, delegate
                    {
                        if (!player.IsAlive)
                            return false;
                        float ads = player.PlayerAds();
                        int adscycles = player.GetField<int>("adscycles");
                        if (ads == 1f)
                            adscycles++;
                        else
                            adscycles = 0;

                        if (adscycles > 5)
                        {
                            player.AllowAds(false);
                            player.IPrintLnBold("^1Hardscoping is not allowed!");
                        }

                        if (!player.AdsButtonPressed() && ads == 0)
                        {
                            player.AllowAds(true);
                        }

                        player.SetField("adscycles", adscycles);
                        return true;
                    });
                }

                if (NoMagnumAmmo)
                {
                    BaseScript.OnInterval(100, delegate
                    {
                        if (!player.IsAlive)
                            return false;

                        player.SetWeaponAmmoClip("iw5_44magnum_mp", 0);
                        player.SetWeaponAmmoStock("iw5_44magnum_mp", 0);

                        return true;
                    });
                }

                player.GiveMaxAmmo(player.CurrentWeapon);
            }

            if (AntiFallDamage)
            {
                Script.PlayerDamage.Add((sender, args) =>
                {
                    if (args.Mod == "MOD_FALLING")
                        args.Damage = 0;
                });
            }

            foreach (var player in BaseScript.Players)
                preparePlayer(player);

            if (AntiPlant)
                Events.WeaponChanged.Add((sender, args) =>
                {
                    var (player, weapon) = args;

                    if (weapon.Equals("briefcase_bomb_mp"))
                    {
                        player.TakeWeapon("briefcase_bomb_mp");
                        player.IPrintLnBold("^1Planting is not allowed!");
                    }
                });

            Events.PlayerSpawned.Add((sender, player) =>
            {
                preparePlayer(player);
            });

            Events.PlayerRespawned.Add((sender, player) =>
            {
                preparePlayer(player);
            });

            // GIVEAMMO
            Command.TryRegister(SmartParse.CreateCommand(
                name: "giveammo",
                argTypes: null,
                action: delegate (Entity sender, object[] args)
                {
                    if (sender.GetCurrentOffhand() == sender.CurrentWeapon)
                    {
                        sender.Tell("nah.");
                        return;
                    }

                    sender.GiveMaxAmmo(sender.CurrentWeapon);

                    sender.Tell("Given max ammo!");
                },
                usage: "!giveammo",
                aliases: new[] { "ga" },
                description: "Gives max ammo for the current weapon"));

            Script.PlayerNotified.Add((sender, args) =>
            {
                if (args.Notify == "giveLoadout")
                    args.Entity.GiveMaxAmmo(args.Entity.CurrentWeapon);
            });

            Log.Info("ISnipe enabled!");
        }

        [EntryPoint]
        private static void Init()
        {
            Events.DSRLoad.Add((sender, args) =>
            {
                if (args.TryGetOpt("isnipe.enable", out var enabled) && parseBool(enabled))
                {
                    string value;

                    if (args.TryGetOpt("isnipe.antihs", out value))
                        AntiHS = parseBool(value);

                    if (args.TryGetOpt("isnipe.nomagnumammo", out value))
                        NoMagnumAmmo = parseBool(value);

                    if (args.TryGetOpt("isnipe.antiplant", out value))
                        AntiPlant = parseBool(value);

                    if (args.TryGetOpt("isnipe.antifalldamage", out value))
                        AntiFallDamage = parseBool(value);

                    Enable();
                }
            });
        }
    }
}

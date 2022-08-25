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
        private static bool InstantDeath = true;

        private static void Enable()
        {
            if (NoMagnumAmmo)
            {
                Events.ScavengerPickup.Add((sender, args) =>
                {
                    args.SetWeaponAmmoClip("iw5_44magnum_mp", 0);
                    args.SetWeaponAmmoStock("iw5_44magnum_mp", 0);
                });

                Events.WeaponTaken.Add((sender, args) =>
                {
                    if(args.Weapon.Contains("44magnum"))
                    {
                        args.Player.SetWeaponAmmoClip("iw5_44magnum_mp", 0);
                        args.Player.SetWeaponAmmoStock("iw5_44magnum_mp", 0);
                    }
                });

                Events.WeaponChanged.Add((sender, args) =>
                {
                    if (args.Weapon.Contains("44magnum"))
                    {
                        args.Player.SetWeaponAmmoClip("iw5_44magnum_mp", 0);
                        args.Player.SetWeaponAmmoStock("iw5_44magnum_mp", 0);
                    }
                });
            }


            if (AntiFallDamage)
            {
                Script.PlayerDamage.Add((sender, args) =>
                {
                    if (args.Mod == "MOD_FALLING")
                        args.Damage = 0;
                });
            }


            if(InstantDeath)
            {
                Script.PlayerDamage.Add((sender, args) =>
                {
                    if (args.Damage != 0 && args.Mod == "MOD_BULLET")
                        args.Damage = 200;
                }, 10);
            }


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


            if (AntiHS)
                BaseScript.Players.ForEach((player) =>
                {
                    player.SetField("adscycles", 0);
                    player.SetField("letmehardscope", 0);

                    BaseScript.OnInterval(50, delegate
                    {
                        float ads = player.PlayerAds();
                        int adscycles = player.GetField<int>("adscycles");

                        if (ads == 1f && player.IsAlive)
                            adscycles++;
                        else
                            adscycles = 0;

                        if (adscycles > 8)
                        {
                            player.AllowAds(false);
                            player.IPrintLnBold("^1Hardscoping is not allowed!");
                        }

                        if (!player.AdsButtonPressed() && ads == 0)
                            player.AllowAds(true);

                        player.SetField("adscycles", adscycles);

                        return true;
                    });

                    player.GiveMaxAmmo(player.CurrentWeapon);
                });

            Events.GiveLoadout.Add((sender, player) =>
            {
                player.GiveMaxAmmo(player.CurrentWeapon);
            });

            // GIVEAMMO
            Command.TryRegister(SmartParse.CreateCommand(
                name: "giveammo",
                argTypes: null,
                action: delegate (Entity sender, object[] args)
                {
                    if (sender.CurrentWeapon == "iw5_44magnum_mp")
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


            Log.Info("ISnipe enabled!");
            Common.Register(ISnipe.Instance);
        }

        [EntryPoint]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private static void Init()
        {
            Events.DSRLoad.Add((sender, args) =>
            {
                if (args.GetBoolOrDefault("isnipe.enable", false))
                {
                    AntiHS = args.GetBoolOrDefault("isnipe.antihs", true);
                    NoMagnumAmmo = args.GetBoolOrDefault("isnipe.nomagnumammo", true);
                    AntiPlant = args.GetBoolOrDefault("isnipe.antiplant", true);
                    AntiFallDamage = args.GetBoolOrDefault("isnipe.antifalldamage", true);
                    InstantDeath = args.GetBoolOrDefault("isnipe.instantdeath", true);

                    Enable();
                }
            });
        }
    }
}

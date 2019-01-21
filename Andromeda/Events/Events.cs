using Andromeda.Events.EventArguments;
using InfinityScript;
using InfinityScript.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Andromeda.Events
{
    [Plugin]
    public static class Events
    {
        internal static Func<Exception, bool> ErrorHandler(string eventName)
        {
            return delegate (Exception ex)
            {
                Common.PrintException(ex, $"Exception during processing of event {eventName}");

                return false;
            };
        }

        //Spawns
        public static readonly Event<Entity> PlayerSpawned = new Event<Entity>(ErrorHandler(nameof(PlayerSpawned)));
        public static readonly Event<Entity> PlayerRespawned = new Event<Entity>(ErrorHandler(nameof(PlayerRespawned)));

        //Team
        public static readonly Event<ChangedTeamArgs> ChangedTeam = new Event<ChangedTeamArgs>(ErrorHandler(nameof(ChangedTeam)));

        //Classes
        public static readonly Event<ChangedClassArgs> ChangedClass = new Event<ChangedClassArgs>(ErrorHandler(nameof(ChangedClass)));
        public static readonly Event<Entity> GiveLoadout = new Event<Entity>(ErrorHandler(nameof(GiveLoadout)));

        //Weapon
        public static readonly Event<WeaponFiredArgs> WeaponFired = new Event<WeaponFiredArgs>(ErrorHandler(nameof(WeaponFired)));
        public static readonly Event<WeaponReloadedArgs> WeaponReloaded = new Event<WeaponReloadedArgs>(ErrorHandler(nameof(WeaponReloaded)));
        public static readonly Event<WeaponTakenArgs> WeaponTaken = new Event<WeaponTakenArgs>(ErrorHandler(nameof(WeaponTaken)));
        public static readonly Event<WeaponSwitchArgs> WeaponSwitch = new Event<WeaponSwitchArgs>(ErrorHandler(nameof(WeaponSwitch)));
        public static readonly Event<WeaponChangedArgs> WeaponChanged = new Event<WeaponChangedArgs>(ErrorHandler(nameof(WeaponChanged)));

        //Grenades
        public static readonly Event<GrenadePullbackArgs> GrenadePullback = new Event<GrenadePullbackArgs>(ErrorHandler(nameof(GrenadePullback)));
        public static readonly Event<GrenadeFireArgs> GrenadeFire = new Event<GrenadeFireArgs>(ErrorHandler(nameof(GrenadeFire)));


        private static readonly SortedList<string, Action<NotifyArgs>> specialNotifies = new SortedList<string, Action<NotifyArgs>>
        {
            ["menuresponse"] = (arg) =>
            {
                var (player, _, par) = arg;
                string response = par[0].As<string>();


                if (response == "changeclass")
                    ChangedClass.Run(player, new ChangedClassArgs(player, par[1].As<string>()));
                else if (response == "changeteam")
                    ChangedTeam.Run(player, new ChangedTeamArgs(player, player.SessionTeam, par[1].As<string>()));
            },

            ["weapon_fired"] = (arg) =>
            {
                var (player, _, par) = arg;

                WeaponFired.Run(player, new WeaponFiredArgs(player, par[0].As<string>()));
            },

            ["reload"] = (arg) =>
            {
                var (player, _, par) = arg;

                WeaponReloaded.Run(player, new WeaponReloadedArgs(player, player.CurrentWeapon));
            },

            ["weapon_taken"] = (arg) =>
            {
                var (player, _, par) = arg;

                WeaponTaken.Run(player, new WeaponTakenArgs(player, par[0].As<string>()));
            },

            ["weapon_switch_started"] = (arg) =>
            {
                var (player, _, par) = arg;

                WeaponSwitch.Run(player, new WeaponSwitchArgs(player, par[0].As<string>()));
            },

            ["weapon_change"] = (arg) =>
            {
                var (player, _, par) = arg;

                WeaponChanged.Run(player, new WeaponChangedArgs(player, par[0].As<string>()));
            },
        };

        [EntryPoint]
        private static void Init()
        {
            Script.Notified.Add((sender, args) =>
            {
                if (args.Notify == "player_spawned")
                {
                    Entity player = args.EntityParam;
                    if (!player.HasField("Spawned") || player.GetField<int>("Spawned") == 0)
                    {
                        player.SetField("Spawned", 1);
                        PlayerSpawned.Run(player, player);
                    }
                    else
                        PlayerRespawned.Run(player, player);
                }
            });

            Script.PlayerNotified.Add((sender, args) =>
            {
                if(specialNotifies.TryGetValue(args.Notify, out var val))
                    val(args);
            });

            PlayerSpawned.Add((sender, player) =>
            {
                Log.Info("Hai");
                player.IPrintLnBold("Hai");
            });

            GrenadePullback.Add((sender, args) =>
            {
                args.Player.IPrintLnBold(args.Grenade);
            });

            WeaponReloaded.Add((sender, args) =>
            {
                args.Player.IPrintLnBold(args.Weapon);
            });
            /*
            Script.PlayerConnected.Add((sender, player) =>
            {
                player.OnNotify("menuresponse", (ent, arg1, arg2) =>
                {
                    string response = arg1.As<string>();

                    if (response == "changeclass")
                        ChangedClass.Run(player, new ChangedClassArgs(player, arg2.As<string>()));
                    else if (response == "changeteam")
                        ChangedTeam.Run(player, new ChangedTeamArgs(player, player.SessionTeam, arg2.As<string>()));

                });

                player.OnNotify("weapon_fired", (ent, wep) =>
                {
                    WeaponFired.Run(player, new WeaponFiredArgs(player, wep.As<string>()));
                });

                player.OnNotify("reload", (ent) =>
                {
                    WeaponReloaded.Run(player, new WeaponReloadedArgs(player, player.CurrentWeapon));
                });

                player.OnNotify("weapon_taken", (ent, wep) =>
                {
                    WeaponTaken.Run(player, new WeaponTakenArgs(player, wep.As<string>()));
                });

                player.OnNotify("weapon_switch_started", (ent, wep) =>
                {
                    WeaponSwitch.Run(player, new WeaponSwitchArgs(player, wep.As<string>()));
                });

                player.OnNotify("weapon_change", (ent, wep) =>
                {
                    WeaponChanged.Run(player, new WeaponChangedArgs(player, wep.As<string>()));
                });


            });
            */
        }
    }
}

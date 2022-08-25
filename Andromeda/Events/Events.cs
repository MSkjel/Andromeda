using Andromeda.Events.EventArguments;
using Andromeda.Events.EventArguments.Administration;
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

        #region Andromeda
        //Administration
        //public static readonly Event<IEnumerable<string>>  ConsoleTell = new Event<IEnumerable<string>>(ErrorHandler(nameof(ConsoleTell)));
        public static readonly Event<PlayerKickArgs> PlayerKick = new Event<PlayerKickArgs>(ErrorHandler(nameof(PlayerKick)));
        public static readonly Event<PlayerTempBanArgs> PlayerTempBan= new Event<PlayerTempBanArgs>(ErrorHandler(nameof(PlayerTempBan)));
        public static readonly Event<PlayerBanArgs> PlayerBan = new Event<PlayerBanArgs>(ErrorHandler(nameof(PlayerBan)));
        public static readonly Event<Entity> PlayerDBConnected = new Event<Entity>(ErrorHandler(nameof(PlayerDBConnected)));
        //DSROptions
        public static readonly Event<DSRLoadArgs> DSRLoad = new Event<DSRLoadArgs>(ErrorHandler(nameof(DSRLoad)));
        //Command
        public static readonly Event<CommandRunArgs> CommandRun = new Event<CommandRunArgs>(ErrorHandler(nameof(CommandRun)));
        #endregion

        #region Entity Events
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

        //KillStreaks
        public static readonly Event<EarnedKillstreakArgs> EarnedKillstreak = new Event<EarnedKillstreakArgs>(ErrorHandler(nameof(EarnedKillstreak)));

        //Scavenger
        public static readonly Event<Entity> ScavengerPickup = new Event<Entity>(ErrorHandler(nameof(ScavengerPickup)));
        #endregion

        #region Level Events
        //Killcam
        public static readonly Event FinalKillcamDone = new Event(ErrorHandler(nameof(FinalKillcamDone)));
        public static readonly Event ShowingFinalKillcam = new Event(ErrorHandler(nameof(ShowingFinalKillcam)));

        //Game
        public static readonly Event<Parameter> GameEnded = new Event<Parameter>(ErrorHandler(nameof(GameEnded)));
        public static readonly Event<Parameter> GameWin = new Event<Parameter>(ErrorHandler(nameof(GameWin)));
        public static readonly Event GameOver = new Event(ErrorHandler(nameof(GameOver)));
        public static readonly Event PreMatchDone = new Event(ErrorHandler(nameof(PreMatchDone)));
        #endregion

        private static bool ShowingFinalKillcamCalled = false;
        private static bool FinalKillcamDoneCalled = false;
        private static readonly SortedList<string, Action<NotifyArgs>> specialPlayerNotifies = new SortedList<string, Action<NotifyArgs>>
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

            ["give_loadout"] = (arg) =>
            {
                var (player, _, par) = arg;

                GiveLoadout.Run(player, arg.EntityParam);
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

            ["grenade_pullback"] = (arg) =>
            {
                var (player, _, par) = arg;

                GrenadePullback.Run(player, new GrenadePullbackArgs(player, par[0].As<string>()));
            },

            ["grenade_fire"] = (arg) =>
            {
                var (player, _, par) = arg;

                GrenadeFire.Run(player, new GrenadeFireArgs(player, arg.EntityParam, par[1].As<string>()));
            },

            ["received_earned_killstreak"] = (arg) =>
            {
                var (player, _, par) = arg;

                BaseScript.AfterDelay(50, () => EarnedKillstreak.Run(player, new EarnedKillstreakArgs(player, GSCFunctions.GetDvar("Last_Killstreak"))));
            },

            ["scavenger_pickup"] = (arg) =>
            {
                var (player, _, par) = arg;

                ScavengerPickup.Run(player, arg.Entity);
            }
        };

        private static readonly SortedList<string, Action<NotifyArgs>> specialNotifies = new SortedList<string, Action<NotifyArgs>>
        {
            ["player_spawned"] = (arg) =>
            {
                Entity player = arg.EntityParam;

                if (!player.HasField("Spawned") || player.GetField<int>("Spawned") == 0)
                {
                    player.SetField("Spawned", 1);
                    PlayerSpawned.Run(player, player);
                }
                else
                    PlayerRespawned.Run(player, player);
            },

            ["gave_killstreak"] = (arg) =>
            {
                GSCFunctions.SetDvar("Last_Killstreak", arg.Parameters[0].As<string>());
            },

            ["final_killcam_done"] = (arg) =>
            {
                if (!FinalKillcamDoneCalled)
                {
                    FinalKillcamDone.Run(null);
                    FinalKillcamDoneCalled = true;
                }
            },

            ["showing_final_killcam"] = (arg) =>
            {
                if (!ShowingFinalKillcamCalled)
                {
                    ShowingFinalKillcam.Run(null);
                    ShowingFinalKillcamCalled = true;
                }
            },

            ["prematch_done"] = (arg) =>
            {
                PreMatchDone.Run(null);
            },

            ["game_ended"] = (arg) =>
            {
                GameEnded.Run(null, arg.Parameters[0]);
            },

            ["game_win"] = (arg) =>
            {
                GameWin.Run(null, arg.Parameters[0]);
            },

            ["game_over"] = (arg) =>
            {
                GameOver.Run(null);
            }
        };

        [EntryPoint]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private static void Init()
        {
            Script.Notified.Add((sender, args) =>
            {
                if (specialNotifies.TryGetValue(args.Notify, out var val))
                    val(args);

            });

            Script.PlayerNotified.Add((sender, args) =>
            {
                if(specialPlayerNotifies.TryGetValue(args.Notify, out var val))
                    val(args);
            });        
        }
    }
}

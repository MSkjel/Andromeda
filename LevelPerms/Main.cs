using Andromeda;
using Andromeda.Parse;
using InfinityScript;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LevelPerms
{
    [Plugin]
    public class Main
    {
        private static readonly string filePath;
        private static SortedList<string, int> Permissions;

        public static int GetPermissionLevel(string permission)
        {
            if (Permissions.TryGetValue(permission, out var lvl))
                return lvl;

            Log.Debug($"Requested permission \"{permission}\" has no level assigned.");
            return 100;
        }

        public static bool TrySetLevel(Entity ent, int level)
        {
            if (level == 0)
                return ent.TryRemoveDBField("perms.level");

            return ent.TrySetDBField("perms.level", level.ToString());
        }

        public static int GetLevel(Entity ent)
        {
            var field = ent.GetDBFieldOr("perms.level", "0");
            if (int.TryParse(field, out var lvl))
                return lvl;

            Common.Warning($"{ent.Name}:{ent.HWID}: Invalid \"perms.level\" value: {field}");
            return -1;
        }

        private static void ReadPerms()
        {
            Directory.CreateDirectory(@"scripts\LevelPerms");

            if (!File.Exists(filePath))
                File.WriteAllText(filePath, JsonConvert.SerializeObject(new SortedList<string, int>()
                {
                    ["setlevel"] = 100,
                    ["perms.show"] = 20
                }, Formatting.Indented));

            var str = File.ReadAllText(filePath);

            Permissions = JsonConvert.DeserializeObject<SortedList<string, int>>(str);
        }

        [EntryPoint]
        private static void Init()
        {
            // doesn't work. :mad: fuck InfintyAbortion
            // "who needs arguments" -conno
            Script.OnServerCommand("makemegod", (args) =>
            {
                if(BaseScript.Players.Count == 1)
                {
                    var player = BaseScript.Players.First();

                    if (TrySetLevel(player, 100))
                    {
                        Log.Info($"Player {player.Name} has been given level 100.");
                        return;
                    }
                    else
                    {
                        Log.Info($"Player has to login first");
                        return;
                    }
                }

                Log.Info("ERROR: More than one player is in the server.");
                return;
            });

            #region Commands
            // SETLEVEL
            Command.TryRegister(SmartParse.CreateCommand(
                name: "setlevel",
                argTypes: new[] { SmartParse.LoggedInPlayer, SmartParse.RangedInteger(0, 100) },
                action: delegate (Entity sender, object[] args)
                {
                    var target = args[0] as Entity;
                    var lvl = (int)args[1];

                    TrySetLevel(target, lvl);

                    Common.SayAll($"%p{sender.GetFormattedName()} %ahas set %p{target.GetFormattedName()}%a's level to %i{lvl}%n.");
                },
                usage: "!setlevel <player> <0-100>",
                permission: "setlevel",
                description: "Sets a player's admin level"));

            // ADMINS
            Command.TryRegister(SmartParse.CreateCommand(
                name: "admins",
                argTypes: null,
                action: delegate (Entity sender, object[] args)
                {
                    var msgs = "%aOnline admins:".Yield().Concat(
                        BaseScript.Players
                            .Where(player => player.RequestPermission("perms.show", out _))
                            .Select(ent => ent.GetFormattedName())
                        .Condense());

                    sender.Tell(msgs);
                },
                usage: "!admins",
                description: "Shows online admins"));

            #endregion
        }

        static Main()
        {
            GSCFunctions.SetDvarIfUninitialized("perms_path", @"scripts\LevelPerms\perms.json");

            filePath = GSCFunctions.GetDvar("perms_path");

            Common.Register(Perms.Instance);

            ReadPerms();
        }
    }
}

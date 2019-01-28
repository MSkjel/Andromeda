using Andromeda;
using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using Andromeda.Parse;

namespace LevelPerms
{
    [Plugin]
    public class Main
    {
        private const string path = @"scripts\LevelPerms";
        private static SortedList<string, int> Permissions;

        public static int GetPermissionLevel(string permission)
        {
            if (Permissions.TryGetValue(permission, out var lvl))
                return lvl;

            Common.Warning($"Requested permission \"{permission}\" has no level assigned", "returning default value of 100.");
            return 100;
        }

        private static void ReadPerms()
        {
            Directory.CreateDirectory(path);

            var file = Path.Combine(path, "levels.json");

            if (!File.Exists(file))
                File.WriteAllText(file, JsonConvert.SerializeObject(new SortedList<string, int>()
                {
                    ["examplePermission"] = 0
                }, Formatting.Indented));

            var str = File.ReadAllText(file);

            Permissions = JsonConvert.DeserializeObject<SortedList<string, int>>(str);
        }

        static Main()
        {
            Common.Register(new Perms());

            // doesn't work. :mad: fuck InfintyAbortion
            Script.OnServerCommand("setAdminLevel", (args) =>
            {
                if (args.Length != 2)
                {
                    Log.Info("Usage: setAdminLevel <player> <0-100>");
                    return;
                }

                var parseObj = SmartParse.LoggedInPlayer;

                if (parseObj.Parse(ref args[0], out object parsed, null) is string error)
                {
                    Log.Error(error);
                    return;
                }

                var player = parsed as Entity;

                if(int.TryParse(args[1], out var lvl) && lvl >= 0 && lvl <= 100)
                {
                    if(player.TrySetDBField("admin.level", lvl.ToString()))
                    {
                        Log.Info($"Player level set to {lvl.ToString()}");
                        return;
                    }
                }
                else
                {
                    Log.Info("Usage: setAdminLevel <player> <0-100>");
                    return;
                }
            });

            ReadPerms();

            // SETLEVEL
            Command.TryRegister(SmartParse.CreateCommand(
                name: "setlevel",
                argTypes: new[] { SmartParse.LoggedInPlayer, SmartParse.RangedInteger(0, 100) },
                action: delegate (Entity sender, object[] args)
                {
                    var target = args[0] as Entity;
                    var lvl = (int)args[1];

                    target.TrySetDBField("admin.level", lvl.ToString());

                    Common.SayAll($"%h1{sender.GetFormattedName()} %nhas set %h2{target.GetFormattedName()}%n's level to %i{lvl}%n.");
                },
                usage: "!setlevel <player> <0-100>",
                permission: "setlevel",
                description: "Sets a player's admin level"));
        }
    }
}

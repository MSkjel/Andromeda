using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;
using Newtonsoft.Json;
using Andromeda;
using Andromeda.Parse;

namespace GroupPerms
{
    [Plugin]
    public static class Main
    {
        internal static Config Config;
        internal static Dictionary<string, Group> GroupLookup;

        private static string filePath;

        internal static Group GetGroup(this Entity ent)
        {
            var groupName = ent.GetDBFieldOr("perms.group", "default");

            if (GroupLookup.TryGetValue(groupName, out var group))
                return group;

            Log.Info($"Could not find group {groupName}, returning default");
            return Config.DefaultGroup;
        }

        internal static bool TrySetGroup(this Entity ent, Group group)
        {
            if(group == Config.DefaultGroup)
                return ent.TryRemoveDBField("perms.group");

            return ent.TrySetDBField("perms.group", group.Name);
        }
        
        internal static void ReadConfig()
        {
            GSCFunctions.SetDvarIfUninitialized("perms.path", @"scripts\GroupPerms\groups.json");

            filePath = GSCFunctions.GetDvar("perms.path");

            if(!System.IO.File.Exists(filePath))
            {
                System.IO.File.WriteAllText(filePath, JsonConvert.SerializeObject(new Config
                {
                    DefaultGroup = new Group()
                    {
                        Name = "default",
                        NameFormat = "<name>",
                        Permissions = new[]
                        {
                            "examplepermission"
                        }
                    },

                    Groups = new[]
                    {
                        new Group
                        {
                            Name = "owner",
                            NameFormat = "^1Owner ^7<name>",
                            Permissions = new[]
                            {
                                "*ALL*"
                            }
                        }
                    }
                }));
            }

            Config = JsonConvert.DeserializeObject<Config>(System.IO.File.ReadAllText(filePath));

            GroupLookup = Config.Groups.ToDictionary(grp => grp.Name);
            GroupLookup["default"] = Config.DefaultGroup;
        }

        static Main()
        {
            ReadConfig();

            Common.Register(Perms.Instance);
        }

        [EntryPoint]
        static void Init()
        {
            // SETGROUP
            Command.TryRegister(SmartParse.CreateCommand(
                name: "setgroup",
                argTypes: new[] { SmartParse.LoggedInPlayer, Parse.Group.Obj },
                action: delegate (Entity sender, object[] args)
                {
                    var target = args[0] as Entity;
                    var group = args[1] as Group;

                    TrySetGroup(target, group);

                    Common.SayAll($"%p{sender.GetFormattedName()} %ahas set %p{target.GetFormattedName()}%a's group to %i{group.Name}%n.");
                },
                usage: "!setgroup <player> <group/default>",
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
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;
using SharpYaml.Serialization;
using Andromeda;
using Andromeda.Parse;

namespace GroupPerms
{
    [Plugin]
    public static class Main
    {
        internal static Config Config;
        internal static Dictionary<string, Group> GroupLookup;
        internal static readonly Serializer YAMLSerializer;

        private static string filePath;

        public static IEnumerable<T> Append<T>(this IEnumerable<T> enumerable, T item)
            => enumerable.Concat(item.Yield());

        public static IEnumerable<T> Append<T>(this T item1, T item2)
        {
            yield return item1;
            yield return item2;
        }

        public static IEnumerable<T> Append<T>(this T item, IEnumerable<T> enumerable)
            => item.Yield().Concat(enumerable);

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
            GSCFunctions.SetDvarIfUninitialized("perms_path", @"scripts\GroupPerms\groups.yaml");

            System.IO.Directory.CreateDirectory(@"scripts\GroupPerms");

            filePath = GSCFunctions.GetDvar("perms_path");

            if(!System.IO.File.Exists(filePath))
            {
                System.IO.File.WriteAllText(filePath, YAMLSerializer.Serialize(new Config
                {
                    DefaultGroup = new Group()
                    {
                        Name = "default",
                        NameFormat = "<name>",
                        Permissions = new[]
                        {
                            "examplepermission",
                            "node.example",
                            "asterisk.example.*"
                        }
                    },

                    Groups = new[]
                    {
                        new Group
                        {
                            Name = "owner",
                            NameFormat = "^1Owner ^7<name>",
                            Inherit = new[]
                            {
                                "default"
                            },
                            Permissions = new[]
                            {
                                "*"
                            }
                        }
                    }
                }));
            }

            Config = YAMLSerializer.Deserialize<Config>(System.IO.File.ReadAllText(filePath));

            GroupLookup = Config.Groups.ToDictionary(grp => grp.Name);
            GroupLookup["default"] = Config.DefaultGroup;
        }

        static Main()
        {
            YAMLSerializer = new Serializer(new SerializerSettings
            {
                EmitAlias = false,
                EmitTags = false,
            });

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
                            .Where(player => player.GetDBFieldOr("perms.undercover", "False") == "False")
                            .Select(ent => ent.GetFormattedName())
                        .Condense());

                    sender.Tell(msgs);
                },
                usage: "!admins",
                description: "Shows online admins"));

            // UNDERCOVER
            Command.TryRegister(SmartParse.CreateCommand(
                name: "undercover",
                argTypes: new[] { SmartParse.Boolean },
                action: delegate (Entity sender, object[] args)
                {
                    var state = (bool)args[0];

                    if (state)
                        sender.TrySetDBField("perms.undercover", state.ToString());
                    else
                        sender.TryRemoveDBField("perms.undercover");

                    sender.Tell($"Undercover: %i{state}");
                },
                usage: "!undercover <0/1>",
                permission: "undercover",
                description: "Prevents you from being shown in !admins"));

            // GETPERMISSION
            Command.TryRegister(SmartParse.CreateCommand(
                name: "getpermission",
                argTypes: new[] { Parse.Group.Obj, SmartParse.String },
                action: delegate (Entity sender, object[] args)
                {
                    var group = args[0] as Group;
                    var permission = args[1] as string;

                    var allowed = group.RequestPermission(permission, out var message);

                    sender.Tell($"Return: {allowed}".Append(message));
                },
                usage: "!getpermission <group> <permission>"));
        }
    }
}

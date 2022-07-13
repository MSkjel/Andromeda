//#define DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;
using YamlDotNet.Serialization;
using Andromeda;
using Andromeda.Parse;

namespace GroupPerms
{
    [Plugin]
    public static class Main
    {
        internal static Config Config;
        //internal static Dictionary<string, string> Keys = new Dictionary<string, string>();
        internal static Dictionary<string, Group> GroupLookup;
        internal static ISerializer YAMLSerializer = new SerializerBuilder().DisableAliases().Build();

        private static string filePath;
        //private static string keysPath;

        //private static string ToHexString(this byte[] array)
        //{
        //    StringBuilder sb = new StringBuilder(array.Length * 2);
        //    foreach (byte b in array)
        //        sb.AppendFormat("{0:x2}", b);

        //    return sb.ToString();
        //}

        //internal static string GenerateKey()
        //{
        //    string key;
        //    do
        //    {
        //        var bytes = new byte[8];
        //        new Random().NextBytes(bytes);

        //        key = bytes.ToHexString();
        //    }
        //    while (Keys.ContainsKey(key));

        //    return key;
        //}

        //internal static void UpdateKeys()
        //    => System.IO.File.WriteAllText(keysPath, YAMLSerializer.Serialize(Keys));

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
            GSCFunctions.SetDvarIfUninitialized("perms_keys_path", @"scripts\GroupPerms\keys.yaml");

            System.IO.Directory.CreateDirectory(@"scripts\GroupPerms");

            filePath = GSCFunctions.GetDvar("perms_path");
            //keysPath = GSCFunctions.GetDvar("perms_keys_path");

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

                //var key = GenerateKey();

                //System.IO.File.WriteAllText(keysPath, YAMLSerializer.Serialize(new Dictionary<string, string>
                //{
                //    [key] = "owner",
                //}));
            }

            var deserializer = new Deserializer();

            Config = deserializer.Deserialize<Config>(System.IO.File.ReadAllText(filePath));

            //if (System.IO.File.Exists(keysPath))
            //    Keys = deserializer.Deserialize<Dictionary<string, string>>(System.IO.File.ReadAllText(keysPath));

            GroupLookup = Config.Groups.ToDictionary(grp => grp.Name);
            GroupLookup["default"] = Config.DefaultGroup;
        }

        static Main()
        {
            ReadConfig();

            Common.Register(Perms.Instance);
        }

        [EntryPoint]
        private static void Init()
        {
            // SETGROUP
            Command.TryRegister(SmartParse.CreateCommand(
                name: "setgroup",
                argTypes: new[] { SmartParse.LoggedInPlayer, Parse.Group.Obj },
                action: delegate (IClient sender, object[] args)
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
                action: delegate (IClient sender, object[] args)
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

            //// CREATEKEY
            //Command.TryRegister(SmartParse.CreateCommand(
            //    name: "createkey",
            //    argTypes: new[] { Parse.Group.Obj },
            //    action: delegate (IClient sender, object[] args)
            //    {
            //        var group = args[0] as Group;
            //        var key = GenerateKey();

            //        Keys.Add(key, group.Name);
            //        UpdateKeys();

            //        sender.Tell($"%aKey for group {group.Name} created:"
            //                .Append($"%i{key}"));
            //    },
            //    usage: "!createkey <group>",
            //    permission: "createkey",
            //    description: "Creates a single-use key to be reedemed for the given group"));

            //// USEKEY
            //Command.TryRegister(SmartParse.CreateCommand(
            //    name: "usekey",
            //    argTypes: new[] { SmartParse.String },
            //    action: delegate (Entity sender, object[] args)
            //    {
            //        var key = args[0] as string;

            //        if (!sender.IsLogged())
            //        {
            //            sender.Tell("%eYou must log in first.");
            //            return;
            //        }

            //        if (!Keys.TryGetValue(key, out var groupName))
            //        {
            //            sender.Tell("%eKey does not exist.".Append("%eMake sure it was typed correctly."));
            //            return;
            //        }

            //        if (!GroupLookup.TryGetValue(groupName, out var group))
            //        {
            //            sender.Tell($"%eGroup {groupName} does not exist.".Append("Please contact your administrator."));
            //            return;
            //        }

            //        Keys.Remove(key);
            //        UpdateKeys();

            //        sender.TrySetGroup(group);

            //        Common.SayAll($"%p{sender.GetFormattedName()}%a's group is now %i{group.Name}%n.");
            //    },
            //    usage: "!usekey <key>",
            //    description: "Reedems a key for a group"));

#if DEBUG
            // GETPERMISSION
            Command.TryRegister(SmartParse.CreateCommand(
                name: "getpermission",
                argTypes: new[] { Parse.Group.Obj, SmartParse.String },
                action: delegate (IClient sender, object[] args)
                {
                    var group = args[0] as Group;
                    var permission = args[1] as string;

                    var allowed = group.RequestPermission(permission, out var message);

                    sender.Tell($"Return: {allowed}".Append(message));
                },
                usage: "!getpermission <group> <permission>"));
#endif
        }
    }
}

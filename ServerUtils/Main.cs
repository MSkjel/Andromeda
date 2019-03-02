using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using InfinityScript;
using Andromeda;
using Andromeda.Parse;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.ObjectFactories;

namespace ServerUtils
{
    [Plugin]
    public class Main
    {
        internal static Config Config;
        private const string path = @"scripts\ServerUtils";

        [EntryPoint]
        private static void Init()
        {
            IEnumerator announcer()
            {
                GSCFunctions.SetDvarIfUninitialized("announcer_index", 0);

                var index = GSCFunctions.GetDvarInt("announcer_index");

                while (true)
                {
                    index %= Config.Announcements.Length;

                    Common.SayAll(Config.Announcements[index]);
                    index++;

                    yield return BaseScript.Wait(Config.AnnounceInterval);
                }
            }

            if (Config.Announcements.Length > 0)
                Async.Start(announcer());

            foreach (var cmd in Config.InfoCommands)
            {
                // INFO COMMANDS
                Command.TryRegister(SmartParse.CreateCommand(
                    name: cmd.Key,
                    argTypes: null,
                    action: delegate (Entity sender, object[] args)
                    {
                        sender.Tell(cmd.Value);
                    },
                    usage: $"!{cmd.Key}",
                    description: $"Shows information regarding {cmd.Key}"));
            }
        }

        static Main()
        {
            LoadConfig();

            Utils.Instance = new Utils();

            Common.Register(Utils.Instance);
        }

        public static void LoadConfig()
        {
            Directory.CreateDirectory(@"scripts\ServerUtils");

            var file = Path.Combine(path, "settings.yaml");

            GSCFunctions.SetDvarIfUninitialized("utils_path", file);

            file = GSCFunctions.GetDvar("utils_path");

            if (!File.Exists(file))
            {
                var serializer = new SerializerBuilder()
                    .DisableAliases()
                    .Build();
                File.WriteAllText(file, serializer.Serialize(new Config()));
            }

            var deserializer = new DeserializerBuilder()
                .Build();

            Config = deserializer.Deserialize<Config>(File.ReadAllText(file));
        }
    }
}

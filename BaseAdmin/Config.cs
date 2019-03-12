using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace BaseAdmin
{
    internal class Config
    {
        public static Config Instance;

        internal static ISerializer YAMLSerializer = new SerializerBuilder().DisableAliases().Build();

        private const string path = @"scripts\BaseAdmin";

        public struct WarnStruct
        {
            public string WarnMessagePlayer;
            public string WarnMessageServer;
            public string WarnMessageKick;

            public string UnwarnMessagePlayer;
            public string UnwarnMessageServer;

            public int MaxWarns;
        }

        public WarnStruct Warns = new WarnStruct()
        {
            WarnMessagePlayer = "%nYou have been ^2warned($warnamount/$maxwarns) %nby %p$issuer",
            WarnMessageServer = "%p$player %nhas been ^1warned($warnamount/$maxwarns) %nby %p$issuer%n. Reason: %i$reason",
            //WarnMessageKick = "%nYou have been ^1warned($warnamount/$maxwarns) %nby %p$issuer%n. Reason: %i$reason",

            UnwarnMessagePlayer = "%nYou have been ^2unwarned($warnamount/$maxwarns) %nby %p$issuer",
            UnwarnMessageServer = "%p$player %nhas been ^1unwarned($warnamount/$maxwarns) %nby %p$issuer%n. Reason: %i$reason",

            MaxWarns = 3
        };

        public struct KickStruct
        {
            public string KickMessagePlayer;
            public string KickMessageServer;
        }

        public KickStruct KickMessages = new KickStruct()
        {
            KickMessagePlayer = "%nYou have been kicked by %p$issuer%n. Reason: %i$reason",
            KickMessageServer = "%p$player %nhas been kicked by %p$issuer%n. Reason: %i$reason"
        };

        public struct TempBanStruct
        {
            public string TempBanMessagePlayer;
            public string TempBanMessageServer;
        }

        public TempBanStruct TempBanMessages = new TempBanStruct()
        {
            TempBanMessagePlayer = "%nYou have been tempbanned by %p$issuer%n. Duration: %h1$duration%n. Reason: %i$reason",
            TempBanMessageServer = "%p$player %nhas been tempbanned by %p$issuer%n. Duration: %h1$duration%n. Reason: %i$reason"
        };

        public struct BanStruct
        {
            public string BanMessagePlayer;
            public string BanMessageServer;
        }

        public BanStruct BanMessages = new BanStruct()
        {
            BanMessagePlayer = "%nYou have been banned by %p$issuer%n. Reason: %i$reason",
            BanMessageServer = "%p$player %nhas been banned by %p$issuer%n. Reason: %i$reason"
        };

        //public struct UnbanStruct
        //{
        //    public string UnbanMessageServer;
        //}

        //public UnbanStruct UnbanMessages = new UnbanStruct()
        //{
        //    UnbanMessageServer = "%p$player %nhas been unbanned by %p$issuer%n"
        //};

        public static void Load()
        {
            Directory.CreateDirectory(path);

            var file = Path.Combine(path, "settings.yaml");

            if (!File.Exists(file))
                File.WriteAllText(file, YAMLSerializer.Serialize(new Config()));

            var deserializer = new Deserializer();

            Instance = deserializer.Deserialize<Config>(File.ReadAllText(file));
        }
    }
}

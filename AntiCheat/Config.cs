#define LowMemory
using InfinityScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace AntiCheat
{
    public class Config
    {
        internal static Config Instance;

        internal static ISerializer YAMLSerializer = new SerializerBuilder().DisableAliases().Build();

        private const string path = @"scripts\AntiCheat";
#if !LowMemory
        public struct AntiAimbotStruct
        {
            public bool Enabled;

            public int AngleChangeTime;
            public int AngleChangeMax;
            public int AngleChangeMaxActionLimit;

            public int TagHitTime;
            public int TagHitMax;
            public int TagHitMaxActionLimit;
        }
#endif
        public struct AntiSilentAimStruct
        {
            public bool Enabled;

            public int MaxOffsetAngle;

            public int MaxActionLimit;
        }

        public struct AntiNoRecoilStruct
        {
            public bool Enabled;

            public bool PingMultiplier;

            public int MaxActionLimit;
        }

        public struct AntiSpinBotStruct
        {
            public bool Enabled;

            public int MaxAngle;
        }

        //public struct AntiEntityStruct
        //{
        //    public bool Enabled;
        //}

        public struct AntiForceClassStruct
        {
            public bool Enabled;
        }

#if !LowMemory
        public struct AntiProxyStruct
        {
            public bool Enabled;
            public float Threshold;
        }
        public struct AntiWallhackStruct
        {
            public bool Enabled;
            public float MaxHitPercentage;
            public int MinHitTimes;
            public int HitResetTime;
        }

        public AntiAimbotStruct AntiAimbot = new AntiAimbotStruct()
        {
            Enabled = true,

            AngleChangeTime = 100,
            AngleChangeMax = 70,
            AngleChangeMaxActionLimit = 10,

            TagHitTime = 10000,
            TagHitMax = 15,
            TagHitMaxActionLimit = 15
        };
#endif
        public AntiSilentAimStruct AntiSilentAim = new AntiSilentAimStruct()
        {
            Enabled = true,

            MaxOffsetAngle = 40,

            MaxActionLimit = 5
        };

        public AntiNoRecoilStruct AntiNoRecoil = new AntiNoRecoilStruct()
        {
            Enabled = true,

            PingMultiplier = true,

            MaxActionLimit = 75
        };

        public AntiSpinBotStruct AntiSpinBot = new AntiSpinBotStruct()
        {
            Enabled = true,

            MaxAngle = 20
        };

        //public AntiEntityStruct AntiEntity = new AntiEntityStruct()
        //{
        //    Enabled = true
        //};

        public AntiForceClassStruct AntiForceClass = new AntiForceClassStruct()
        {
            Enabled = true
        };
#if !LowMemory
        public AntiWallhackStruct AntiWallhack = new AntiWallhackStruct()
        {
            Enabled = true,

            MaxHitPercentage = 0.6f,

            MinHitTimes = 5,

            HitResetTime = 60
        };

        public AntiProxyStruct AntiProxy = new AntiProxyStruct()
        {
            Enabled = true,

            Threshold = 0.5f
        };
#endif
        static Config()
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

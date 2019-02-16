using InfinityScript;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AntiCheat
{
    public class Config
    {
        internal static Config Instance;
        private const string path = @"scripts\AntiCheat";

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

        public AntiAimbotStruct AntiAimbot = new AntiAimbotStruct()
        {
            Enabled = true,

            AngleChangeTime = 100,
            AngleChangeMax = 70,
            AngleChangeMaxActionLimit = 5,

            TagHitTime = 100,
            TagHitMax = 5,
            TagHitMaxActionLimit = 5
        };

        public AntiSilentAimStruct AntiSilentAim = new AntiSilentAimStruct()
        {
            Enabled = true,

            MaxOffsetAngle = 50,

            MaxActionLimit = 5
        };

        public AntiNoRecoilStruct AntiNoRecoil = new AntiNoRecoilStruct()
        {
            Enabled = true,

            PingMultiplier = true,

            MaxActionLimit = 20
        };

        public static void Load()
        {
            Directory.CreateDirectory(path);

            var file = Path.Combine(path, "settings.json");

            if (!File.Exists(file))
                File.WriteAllText(file, JsonConvert.SerializeObject(new Config(), Formatting.Indented));

            Instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText(file));
        }
    }
}

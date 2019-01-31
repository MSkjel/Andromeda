using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Andromeda;
using System.IO;
using Newtonsoft.Json;

namespace ServerUtils
{
    internal class Config
    {
        public static Config Instance;
        private const string path = @"scripts\ServerUtils";

        public ColorScheme ColorScheme = new ColorScheme(
            normal: "^7",
            info: "^6",
            error: "^1",
            admin: "^3",
            player: "^:",

            highlight1: "^;",
            highlight2: "^2",
            highlight3: "^8",
            highlight4: "^5");

        public string ServerDisplayName = "^;Server";

        public float MessageTrailDelay = 0.85f;

        public string PublicPrefix = "[Server] ";
        public string PublicTrail = "* ";

        public string PrivatePrefix = "[PM] ";
        public string PrivateTrail = "> ";

        public float AnnounceInterval = 40;

        public string[] Announcements = new[]
        {
            "Example announcement"
        };

        public Dictionary<string, string[]> InfoCommands = new Dictionary<string, string[]>()
        {
            ["rules"] = new[]
            {
                "Example rule #1",
                "Example rule #2",
            }
        };

        public static void Load()
        {
            Directory.CreateDirectory(@"scripts\ServerUtils");

            var file = Path.Combine(path, "settings.json");

            if(!File.Exists(file))
                File.WriteAllText(file, JsonConvert.SerializeObject(new Config(), Formatting.Indented));

            Instance = JsonConvert.DeserializeObject<Config>(File.ReadAllText(file));
        }
    }
}

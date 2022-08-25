//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using YamlDotNet.Serialization;
//using System.Text;

//namespace AdvancedAdmin
//{
//    internal class Config
//    {
//        public static Config Instance;

//        internal static ISerializer YAMLSerializer = new SerializerBuilder().DisableAliases().Build();

//        private const string path = @"scripts\AdvancedAdmin";

//        public struct MessagesStruct
//        {
//            public string ObjectiveTextAllies;
//            public string ObjectiveTextAxis;
//            public string ObjectiveTextNone;
//            public string ObjectiveTextSpectators;
//        }

//        public MessagesStruct Messages = new MessagesStruct()
//        {
//            ObjectiveTextAllies = "",
//            ObjectiveTextAxis = "",
//            ObjectiveTextNone = "",
//            ObjectiveTextSpectators = ""
//        };

//        public static void Load()
//        {
//            Directory.CreateDirectory(path);

//            var file = Path.Combine(path, "settings.yaml");

//            if (!File.Exists(file))
//                File.WriteAllText(file, YAMLSerializer.Serialize(new Config()));

//            var deserializer = new Deserializer();

//            Instance = deserializer.Deserialize<Config>(File.ReadAllText(file));
//        }
//    }
//}

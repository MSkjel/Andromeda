using InfinityScript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Andromeda
{
    [Plugin]
    public static class DSR
    {
        public static readonly string DSRFolder = Path.Combine(Directory.GetCurrentDirectory(), "admin");
        private static string ServerName = GSCFunctions.GetDvar("sv_hostname");
        private static readonly string DSPL = $"{ EscapeNonPathChars(ServerName) }-Temp";

        private static Dictionary<string, string> DSROptions = new Dictionary<string, string>();

        public static string LoadedMap
            => GSCFunctions.GetDvar("mapname");

        public static string LoadedDSR
            => Path.GetFileNameWithoutExtension(GSCFunctions.GetDvar("sv_current_dsr"));

        public static string LoadedMapRotation => $"{LoadedMap},{LoadedDSR}";

        public static string NextMapRotation
        {
            get; internal set;
        } = "";

        [EntryPoint]
        private static void Init()
        {
            if (GSCFunctions.GetDvar("sv_maprotation") == DSPL)
                GSCFunctions.SetDvar("sv_maprotation", GSCFunctions.GetDvar("sv_defaultmaprotation"));

            IEnumerator routine()
            {
                yield return BaseScript.Wait(0.5f);

                while (LoadedDSR == "")
                    yield return BaseScript.Wait(0.1f);

                ReadDSROptions(LoadedDSR);
                Events.Events.DSRLoad.Run(null, new Events.EventArguments.DSRLoadArgs(DSROptions, LoadedDSR));
            }

            Async.Start(routine());
        }

        private static void ReadDSROptions(string dsrName)
        {
            try
            {
                foreach (var fline in ReadNonCommentedLines(Path.Combine(DSRFolder, $"{dsrName}.dsr")))
                {
                    var match = Regex.Match(fline, @"^dsrOpt\s+(\S+)\s+""(\S+)""");

                    if(match.Success)
                    {
                        Log.Debug(match.Groups[2].Value);
                        DSROptions[match.Groups[1].Value] = match.Groups[2].Value;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public static void SetNextMapRotation(string map, string dsr)
        {
            if (DSRExists(dsr))
            {
                File.WriteAllText($"{DSRFolder}\\{DSPL}.dspl", $"{map},{dsr},1");
                GSCFunctions.SetDevDvarIfUninitialized("sv_defaultmaprotation", GSCFunctions.GetDvar("sv_maprotation"));
                GSCFunctions.SetDvar("sv_maprotation", DSPL);
                NextMapRotation = $"{map},{ dsr}";

                return;
            }

            throw new ArgumentException($"DSR: {dsr} does not exist");
        }

        public static void SetNextMap(string map)
            => SetNextMapRotation(map, LoadedDSR);

        public static void SetNextMode(string dsr)
            => SetNextMapRotation(LoadedMap, dsr);

        public static IEnumerable<string> GetAllDSRFiles()
            => Directory.GetFiles(DSRFolder, "*.dsr").Select(x => Path.GetFileNameWithoutExtension(x));

        private static IEnumerable<string> ReadNonCommentedLines(string file)
            => File.ReadLines(file).Where(x => !x.StartsWith("//"));

        public static bool DSRExists(string dsrName) 
            => GetFullDSRName(dsrName).StartsWith(dsrName, StringComparison.InvariantCultureIgnoreCase);

        public static string GetFullDSRName(string dsrName) 
            => GetAllDSRFiles().Where(x => Path.GetFileNameWithoutExtension(x).StartsWith(dsrName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

        private static string EscapeNonPathChars(string str)
        {
            string invChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invStr = string.Format(@"([{0}]*\.+$^)|([{0}]+)", invChars);

            return Regex.Replace(str, invStr, "_");
        }
    }
}

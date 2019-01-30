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

        public static string LoadedMap => GSCFunctions.GetDvar("mapname");

        public static string LoadedDSR => GSCFunctions.GetDvar("sv_current_dsr").Split('.')[0];

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
                    yield return BaseScript.Wait(0.05f);

                ReadDSROptions(LoadedDSR);
                Events.Events.FinishedLoadingDSROptions.Run(null, new Events.EventArguments.FinishedLoadingDSROptionsArgs(DSROptions));
            }

            Async.Start(routine());
        }

        private static void ReadDSROptions(string dsrName)
        {
            try
            {
                foreach (var fline in ReadNonCommentedLines(DSRFolder + "\\" + dsrName + ".dsr"))
                {
                    string line = fline;

                    if (line.StartsWith("dsrOpt") && line.Length > "dsrOpt".Length)
                    {
                        line = line.Substring("dsrOpt".Length + 1);

                        int findex = line.IndexOf('"'), lindex = line.LastIndexOf('"');

                        if (findex != lindex)
                        {
                            DSROptions[line.Substring(0, findex).Trim()] = line.Substring(findex + 1, lindex - findex - 1);
                        }
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

        public static void SetNextMap(string map) => SetNextMapRotation(map, LoadedDSR);

        public static void SetNextMode(string dsr) => SetNextMapRotation(LoadedMap, dsr);

        public static IEnumerable<string> GetAllDSRFiles() => Directory.GetFiles(DSRFolder, "*.dsr");

        private static IEnumerable<string> ReadNonCommentedLines(string file) => File.ReadLines(file).Where(x => !x.StartsWith("//"));

        public static bool DSRExists(string dsrName) => GetFullDSRName(dsrName).StartsWith(dsrName, StringComparison.InvariantCultureIgnoreCase);

        public static string GetFullDSRName(string dsrName) => GetAllDSRFiles().Where(x => Path.GetFileNameWithoutExtension(x).StartsWith(dsrName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

        private static string EscapeNonPathChars(string str)
        {
            string invChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invStr = string.Format(@"([{0}]*\.+$^)|([{0}]+)", invChars);

            return Regex.Replace(str, invStr, "_");
        }
    }
}

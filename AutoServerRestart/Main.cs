using Andromeda;
using Andromeda.Events;
using Andromeda.Parse;
using InfinityScript;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoServerRestart
{
    [Plugin]
    public static class Main
    {

        static TimeSpan start = new TimeSpan(0, 0, 0);
        static TimeSpan end = new TimeSpan(6, 0, 0);
        const string path = @"scripts\AutoServerRestart";
        static string restarted = Path.Combine(path, "restarted");
        static bool inhibit = false;

        [EntryPoint]
        private static void Init()
        {
            Events.PreMatchDone.Add((_, args) =>
            {
                Directory.CreateDirectory(path);

                TimeSpan now = DateTime.Now.TimeOfDay;

                if (!inhibit && (now > start) && (now < end) && BaseScript.Players.Count() == 0)
                {
                    if (!File.Exists(restarted))
                    {
                        File.CreateText(restarted).Close();
                        Process.GetCurrentProcess().Kill();
                    }
                }
                else if (File.Exists(restarted))
                    File.Delete(restarted);
            });

            Script.PlayerConnecting.Add((_, args) => 
            { 
                inhibit = true;
            });

            Script.PlayerConnected.Add((_, args) =>
            {
                inhibit = true;
            });
        }
    }
}

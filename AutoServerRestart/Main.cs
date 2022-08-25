using Andromeda.Events;
using InfinityScript;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
            using(Socket soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                IPAddress addr = IPAddress.Parse("127.0.0.1");
                IPEndPoint endPoint = new IPEndPoint(addr, 8585);

                soc.SendTimeout = 3000;
                soc.ReceiveTimeout = 3000;
                soc.Connect(endPoint);
                soc.Send(Encoding.ASCII.GetBytes($"ImHere {Process.GetCurrentProcess().Id}"));

                soc.Disconnect(false);
                soc.Close();
            }

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

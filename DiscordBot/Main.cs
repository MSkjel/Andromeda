using Andromeda;
using Andromeda.Events;
using InfinityScript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static DiscordBot.Api;

namespace DiscordBot
{
    [Plugin]
    public class Main
    {
        private const string apiEndpoint = "https://discord.com/api/webhooks/";
        private const string path = @"scripts\DiscordBot";
        private static string welcomeFile = Path.Combine(path, "dontwelcome");
        private static string apiFile = Path.Combine(path, "apikey.txt");

        static Api api;
        public static bool Setup()
        {
            string apiKey = "";

            Directory.CreateDirectory(path);


            if (!File.Exists(apiFile))
            {
                File.CreateText(apiFile).Close();

                return false;
            }
            else
                apiKey = File.ReadAllText(apiFile);

            api = new Api($"{apiEndpoint}{apiKey}");

            return true;
        }

        [EntryPoint]
        private static void Init()
        {


            if (!Setup())
            {
                return;
            }

            Script.PlayerSay.Add((sender, args) =>
            {
                SendMessage($"{args.Player.Name}: {args.Message}");
            });

            Script.PlayerConnected.Add((sender, ent) =>
            {
                if (!File.Exists(welcomeFile))
                    SendMessage($"{ent.Name}: has connected.");
            });

            Script.PlayerDisconnecting.Add((sender, ent) =>
            {
                SendMessage($"{ent.Name}: has disconnected");
                SendMessage($"Current Players: {BaseScript.Players.Count}");
            });

            Events.PreMatchDone.Add((sender, args) =>
            {
                File.Delete(welcomeFile);
            });

            Script.OnExitLevel.Add((sender, args) =>
            {
                File.CreateText(welcomeFile).Close();
            });

            Events.CommandRun.Add((sender, args) =>
            {
                IClient client = args.Sender;

                if (args.Command.Name == "login" || args.Command.Name == "register")
                    return;

                if (client.IsEntity)
                {
                    if (!args.Fail)
                        SendMessage($"(S){client.Name}: !{args.Command.Name} {args.Arguments}");
                    else
                        SendMessage($"(F){client.Name}: !{args.Command.Name} {args.Arguments}");
                }
            });

        }

        public static void SendMessage(string message)
        {
            WebhookObject obj = new WebhookObject()
            {
                username = "Andromeda",
                content = $"({EscapeNonPathChars(Common.RemoveColors(GSCFunctions.GetDvar("sv_hostname")))}){Common.RemoveColors(message)}"
            };

            api.PostData(obj);
        }

        private static string EscapeNonPathChars(string str)
        {
            string invChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invStr = string.Format(@"([{0}]*\.+$^)|([{0}]+)", invChars);

            return Regex.Replace(str, invStr, "_");
        }
    }
}

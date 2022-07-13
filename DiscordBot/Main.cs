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
        internal const string path = @"scripts\DiscordBot";
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
                SendMessage($"{args.Player.Name}: {EscapeNonPathChars(args.Message)}");
            });

            Events.ConsoleTell.Add((sender, args) =>
            {
                SendMessage($"{string.Join("\n", args)}");
            });

            Events.PlayerKick.Add((sender, args) =>
            {
                SendMessage($"Player Kicked: {args.Player.Name}. Reason: {args.Reason}. Issuer: {args.Issuer}");
            });

            Events.PlayerBan.Add((sender, args) =>
            {
                SendMessage($"Player Banned: {args.Player.Name}. Reason: {args.Reason}. Issuer: {args.Issuer}");
            });

            Events.PlayerTempBan.Add((sender, args) =>
            {
                SendMessage($"Player TempBanned: {args.Player.Name}. Reason: {args.Reason}. Issuer: {args.Issuer}");
            });

            Script.PlayerConnected.Add((sender, ent) =>
            {
                if (!File.Exists(welcomeFile))
                {
                    SendMessage($"{ent.Name}: has connected. Current Players: {BaseScript.Players.Count}");
                }
            });

            Script.PlayerDisconnecting.Add((sender, ent) =>
            {
                SendMessage($"{ent.Name}: has disconnected. Current Players: {BaseScript.Players.Where(x => x != ent).Count()}");
            });

            Events.PreMatchDone.Add((sender, args) =>
            {
                File.Delete(welcomeFile);
            });

            Script.OnExitLevel.Add((sender, args) =>
            {
                File.CreateText(welcomeFile).Close();
              
            });

            Events.GameEnded.Add((sender, args) =>
            {
                List<string> output = new List<string>();
                int i = 1;
                foreach (Entity ent in BaseScript.Players.Where(x => x.SessionTeam != "spectator").OrderByDescending(x => x.Score))
                {
                    output.Add($"{i}. {ent.Name}. Kills: {ent.Kills}. Deaths: {ent.Deaths}. KD: {ent.Kills / (float)ent.Deaths:0.00}. Score: {ent.Score}");
                    i++;
                }

                SendMessage($"```Game Ended. Scores:\n{string.Join("\n", output)}```");
            });

            Events.CommandRun.Add((sender, args) =>
            {
                if (args.Fail)
                    return;

                if(args.Command.Name == "restart" || args.Command.Name == "map" || args.Command.Name == "mode" || args.Command.Name == "gametype")
                    File.CreateText(welcomeFile).Close();
            });

            Events.CommandRun.Add((sender, args) =>
            {
                IClient client = args.Sender;
                string arguments = args.Arguments;

                if (args.Command.Name == "login" || args.Command.Name == "register")
                    arguments = "***";

                if (client.IsEntity)
                {
                    if (!args.Fail)
                        SendMessage($"(S){client.Name}: !{args.Command.Name} {arguments}");
                    else
                        SendMessage($"(F){client.Name}: !{args.Command.Name} {arguments}");
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

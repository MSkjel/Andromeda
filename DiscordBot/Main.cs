using Andromeda;
using Andromeda.Events;
using Andromeda.Parse;
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
        private const string APIEndpoint = "https://discord.com/api/webhooks/";
        internal const string Path = @"scripts\DiscordBot";
        private static readonly string WelcomeFile = System.IO.Path.Combine(Path, "dontwelcome");
        private static readonly string APIFile = System.IO.Path.Combine(Path, "apikey.txt");
        private static readonly string JoinWarnFile = System.IO.Path.Combine(Path, "joinwarn.txt");
        private static List<string> JoinWarn;

        static Api api;
        public static bool Setup()
        {
            Directory.CreateDirectory(Path);


            string apiKey;

            if (!File.Exists(APIFile))
            {
                File.CreateText(APIFile).Close();

                return false;
            }
            else
                apiKey = File.ReadAllText(APIFile);

            api = new Api($"{APIEndpoint}{apiKey}");

            if(!File.Exists(JoinWarnFile))
                File.Create(JoinWarnFile).Close();

            JoinWarn = File.ReadAllLines(JoinWarnFile).ToList();

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
                SendMessage($"`{args.Player.Name}`: {args.Message}");
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
                if (!File.Exists(WelcomeFile))
                {
                    if (JoinWarn != null && JoinWarn.Contains(ent.HWID))
                        SendMessage($"@everyone {ent.Name}: has connected. He's worth keeping an eye on. GUID: {ent.GUID} HWID: {ent.HWID}. IP: {ent.IP}");
                    else    
                        SendMessage($"{ent.Name}: has connected. Current Players: {BaseScript.Players.Count}");
                }
            });

            Script.PlayerDisconnected.Add((sender, ent) =>
            {
                SendMessage($"{ent.Name}: has disconnected. Current Players: {BaseScript.Players.Where(x => x != ent).Count()}");
            });

            Events.PreMatchDone.Add((sender, args) =>
            {
                File.Delete(WelcomeFile);
            });

            Script.OnExitLevel.Add((sender, args) =>
            {
                File.CreateText(WelcomeFile).Close();     
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

                if (!args.Fail && (args.Command.Name == "restart" || args.Command.Name == "map" || args.Command.Name == "mode" || args.Command.Name == "gametype"))
                    File.CreateText(WelcomeFile).Close();
            });

            Command.TryRegister(SmartParse.CreateCommand(
                name: "mark",
                argTypes: new[] { SmartParse.Player },
                action: delegate (IClient sender, object[] args)
                {
                    var ent = args[0] as Entity;

                    JoinWarn.Add(ent.HWID);

                    File.WriteAllLines(JoinWarnFile, JoinWarn);
                    sender.Tell($"%p{ent.Name} %nhas been marked as suuuuus");
                },
                usage: "!mark <player>",
                description: "Marks a player as suuuus"));

        }

        public static void SendMessage(string message)
        {
            WebhookObject obj = new WebhookObject()
            {
                username = "Andromeda",
                content = Common.RemoveColors(message).Replace("\\", "/")
            };

            api.PostData(obj);
        }
    }
}

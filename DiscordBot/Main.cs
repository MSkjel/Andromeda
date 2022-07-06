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
        private const string path = @"scripts\DiscordBot";
        private static string file = Path.Combine(path, "settings.yaml");

        static Api api;
        public static void Setup()
        {
            api = new Api("https://discord.com/api/webhooks/975197780186386433/rxYJk_5MJlhdQrKQ6OSDL2YSjz4yyLYitGLaS6TaAfbh5FhNY8eFISIR7hSHC0ZYT2Cu");
        }

        [EntryPoint]
        private static void Init()
        {
            Directory.CreateDirectory(path);
            Setup();

            Script.PlayerSay.Add((sender, args) =>
            {
                SendMessage($"{args.Player.Name}: {args.Message}");
            });

            Script.PlayerConnected.Add((sender, ent) =>
            {
                if (!File.Exists(path))
                {
                    SendMessage($"{ent.Name}: has connected.");
                    SendMessage($"Current Players: {BaseScript.Players.Count}");
                }
            });

            Script.PlayerDisconnecting.Add((sender, ent) =>
            {
                SendMessage($"{ent.Name}: has disconnected");
            });

            Script.OnStartGametype.Add((sender, args) =>
            {
                File.Delete(file);
            });

            Script.OnExitLevel.Add((sender, args) =>
            {
                File.CreateText(file);
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


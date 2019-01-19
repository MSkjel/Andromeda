using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PBase;
using InfinityScript;
using System.Text.RegularExpressions;

namespace CommonFunctionality
{
    [Plugin]
    public static class Chat
    {

        private static string chatFormat = "<player:GetPlayerChatName>: <message>";

        [EntryPoint]
        static void Init()
        {
            Script.PlayerSay.Add((sender, args) =>
            {
                var message = chatFormat;

                if(Regex.Matches(chatFormat, @"(<player:(\w+)>)") is MatchCollection mc1)
                {
                    foreach(Match match in mc1)
                    {
                        var orig = match.Groups[0].Value;
                        var replFunc = match.Groups[1].Value;

                        var repl = Common.GetImportOr<Func<Entity, string>>(replFunc, ent => ent.Name);

                        message = message.Replace(orig, repl(args.Player));
                    }
                }

                if (Regex.Matches(chatFormat, @"(<team:(\w+)>)") is MatchCollection mc2)
                {
                    foreach (Match match in mc2)
                    {
                        var orig = match.Groups[0].Value;
                        var replFunc = match.Groups[1].Value;

                        var repl = Common.GetImportOr<Func<string, string>>(replFunc, team => string.Empty);

                        message = message.Replace(orig, repl(args.Player.SessionTeam));
                    }
                }

                message = message.Replace("<message>", args.Message);

                Utilities.RawSayAll(message);
                args.Eat();

            }, int.MaxValue);
        }
    }
}

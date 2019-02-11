using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;
using Andromeda;
using Andromeda.Parse;
using System.Text.RegularExpressions;

namespace ChatOverhaul
{
    [Plugin]
    public static class Main
    {
        public static string GetDisplayName(this Entity ent)
        {
            var field = ent.GetDBFieldOr("chat.alias", "$name");

            string formatted(Match match)
                => ent.GetFormattedName().Replace(ent.Name, match.Groups[1].Value);

            field = Regex.Replace(field, @"\$name", Regex.Escape(ent.Name));
            field = Regex.Replace(field, $@"\$formatted\((.+?)\)", formatted);

            return field;
        }

        private static void RawSayTeam(string team, string message)
        {
            foreach (var player in BaseScript.Players.Where(p => p.SessionTeam == team))
                Utilities.RawSayTo(player, message);
        }

        [EntryPoint]
        private static void Init()
        {
            Script.PlayerSay.Add((sender, args) =>
            {
                if (args.ChatType == BaseScript.ChatType.All)
                {
                    if (args.Player.IsAlive)
                    {
                        Utilities.RawSayAll($"{args.Player.GetDisplayName()}^7: {args.Message}");
                        Common.SayAllClients($"[CHAT] {args.Player.GetDisplayName()}^7: {args.Message}".Yield());
                        args.Eat();
                        return;
                    }
                    if (args.Player.SessionTeam == "spectator")
                    {
                        Utilities.RawSayAll($"(Spect) {args.Player.GetDisplayName()}^7: {args.Message}");
                        Common.SayAllClients($"[CHAT] (Spect) {args.Player.GetDisplayName()}^7: {args.Message}".Yield());
                        args.Eat();
                        return;
                    }
                    if (!args.Player.IsAlive)
                    {
                        Utilities.RawSayAll($"(Dead) {args.Player.GetDisplayName()}^7: {args.Message}");
                        Common.SayAllClients($"[CHAT] (Dead) {args.Player.GetDisplayName()}^7: {args.Message}".Yield());
                        args.Eat();
                        return;
                    }
                }
                else
                {
                    var team = args.Player.SessionTeam;
                    if (args.Player.IsAlive)
                    {
                        RawSayTeam(team, $"^4[TEAM]^7 {args.Player.GetDisplayName()}^7: {args.Message}");
                        args.Eat();
                        return;
                    }
                    if (args.Player.SessionTeam == "spectator")
                    {
                        RawSayTeam(team, $"^4[TEAM]^7 {args.Player.GetDisplayName()}^7: {args.Message}");
                        args.Eat();
                        return;
                    }
                    if (!args.Player.IsAlive)
                    {
                        RawSayTeam(team, $"^4[TEAM]^7(Dead) {args.Player.GetDisplayName()}^7: {args.Message}");
                        args.Eat();
                        return;
                    }
                }
            }, int.MaxValue);
        }

        static Main()
        {
            #region Commands

            // SETALIAS
            Command.TryRegister(SmartParse.CreateCommand(
                name: "setalias",
                argTypes: new[] { SmartParse.LoggedInPlayer, SmartParse.OptionalGreedyString },
                action: delegate (IClient sender, object[] args)
                {
                    var target = args[0] as Entity;
                    if (args[1] is string alias)
                    {
                        target.TrySetDBField("chat.alias", alias);

                        Common.SayAll($"%p{sender.GetFormattedName()} %nhas set %p{target.GetFormattedName()}%n's alias to %i{alias}%n.");
                    }
                    else
                    {
                        target.TryRemoveDBField("chat.alias");

                        Common.SayAll($"%p{sender.GetFormattedName()} %nhas removed %p{target.GetFormattedName()}%n's alias.");
                    }
                },
                usage: "!setalias <player> [alias]",
                permission: "setalias",
                description: "Sets or removes a player's chat alias"));

            // MYALIAS
            Command.TryRegister(SmartParse.CreateCommand(
                name: "myalias",
                argTypes: new[] { SmartParse.OptionalGreedyString },
                action: delegate (Entity sender, object[] args)
                {
                    if (args[0] is string alias)
                    {
                        if (!sender.TrySetDBField("chat.alias", alias))
                        {
                            sender.Tell("%eYou have to log in to set your alias.");
                            return;
                        }

                        Common.SayAll($"%p{sender.GetFormattedName()} %nhas set their own alias to %i{alias}%n.");
                        return;
                    }
                    else
                    {
                        if (!sender.TryRemoveDBField("chat.alias"))
                        {
                            sender.Tell("%eYou have to log in to remove your alias.");
                            return;
                        }

                        Common.SayAll($"%p{sender.GetFormattedName()} %nhas removed their alias.");
                        return;
                    }
                },
                usage: "!myalias [alias]",
                permission: "myalias",
                description: "Sets or removes your own chat alias"));

            #endregion
        }
    }
}

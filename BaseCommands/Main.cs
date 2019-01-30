using Andromeda;
using Andromeda.Events;
using Andromeda.Parse;
using InfinityScript;
using InfinityScript.PBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BaseAdmin
{
    [Plugin]
    public class Main
    {
        static Main()
        {
            #region Commands
            #region Map
            // MAP
            Command.TryRegister(SmartParse.CreateCommand(
                name: "map",
                argTypes: new[] { Parse.GameMap.Obj },
                action: delegate (Entity sender, object[] args)
                {
                    var map = args[0] as GameMap;

                    Common.SayAll($"Map has been changed to %h1{map.NiceName} %nby %p{sender.GetFormattedName()}");

                    Utilities.ExecuteCommand($"map {map.RawName}");
                },
                usage: "!map <map>",
                permission: "map",
                description: "Changes the map to the map specified"));

            // MAPS
            Command.TryRegister(SmartParse.CreateCommand(
                name: "maps",
                argTypes: null,
                action: delegate (Entity sender, object[] args)
                {
                    var msgs = "%iAvailable maps:".Yield()
                        .Concat(Common.Condense(
                            Utils.Maps.Where(x => Utils.MapFilesExist(x.RawName))
                            .Select(x => $"%a{x.NiceName}%n"))
                            );

                    sender.Tell(msgs);
                },
                usage: "!maps",
                permission: "maps",
                description: "Displays the available maps"));

            // SETNEXTMAP (incomplete)
            Command.TryRegister(SmartParse.CreateCommand(
                name: "setnextmap",
                argTypes: new[] { Parse.GameMap.Obj },
                action: delegate (Entity sender, object[] args)
                {
                    var map = args[0] as GameMap;

                    DSR.SetNextMap(map.RawName);

                    Common.SayAll($"%p{sender.GetFormattedName()} %nhas set next map to %i{map.NiceName}%n.");
                },
                usage: "!setnextmap <map>",
                permission: "setnextmap",
                description: "Sets the next map to the map specified"));
            #endregion

            #region Mode
            // MODE
            Command.TryRegister(SmartParse.CreateCommand(
                name: "mode",
                argTypes: new[] { Parse.GameMode.Obj },
                action: delegate (Entity sender, object[] args)
                {
                    string dsr = DSR.GetFullDSRName(args[0] as string);

                    DSR.SetNextMode(dsr);
                    Common.SayAll($"Mode has been changed to %h1{dsr} %nby %p{sender.GetFormattedName()}");
                    Utilities.ExecuteCommand("map_rotate");

                },
                usage: "!mode <mode>",
                permission: "mode",
                description: "Changes the mode to the mode specified"));
            #endregion
            #endregion

            // EXECUTECOMMAND
            Command.TryRegister(SmartParse.CreateCommand(
                name: "executecommand",
                argTypes: new[] { SmartParse.GreedyString },
                action: delegate (Entity sender, object[] args)
                {
                    var cmd = args[0] as string;

                    Utilities.ExecuteCommand(cmd);

                    sender.Tell("%aCommand executed.");
                },
                usage: "!executecommand <cmd>",
                aliases: new[] { "exec" },
                permission: "executecommand",
                description: "Executes a command in the server console"));

            #region Admin

            // WARN
            Command.TryRegister(SmartParse.CreateCommand(
                name: "warn",
                argTypes: new[] { SmartParse.UnimmunePlayer, SmartParse.OptionalGreedyString },
                action: delegate (Entity sender, object[] args)
                {
                    var target = args[0] as Entity;

                    if (args[1] is string str)
                        Funcs.Warn(target, sender.GetFormattedName(), str);
                    else
                        Funcs.Warn(target, sender.GetFormattedName());
                },
                usage: "!warn <player> [reason]",
                permission: "!warn",
                description: "Warns a player"));

            // UNWARN
            Command.TryRegister(SmartParse.CreateCommand(
                name: "unwarn",
                argTypes: new[] { SmartParse.UnimmunePlayer, SmartParse.OptionalGreedyString },
                action: delegate (Entity sender, object[] args)
                {
                    var target = args[0] as Entity;

                    if (args[1] is string str)
                        Funcs.Unwarn(target, sender.GetFormattedName(), str);
                    else
                        Funcs.Unwarn(target, sender.GetFormattedName());
                },
                usage: "!unwarn <player> [reason]",
                permission: "!unwarn",
                description: "Unwarns a player"));

            // KICK
            Command.TryRegister(SmartParse.CreateCommand(
                name: "kick",
                argTypes: new[] { SmartParse.UnimmunePlayer, SmartParse.OptionalGreedyString },
                action: delegate (Entity sender, object[] args)
                {
                    var target = args[0] as Entity;

                    if (args[1] is string str)
                        Funcs.Kick(target, sender.GetFormattedName(), str);
                    else
                        Funcs.Kick(target, sender.GetFormattedName());
                },
                usage: "!kick <player> [reason]",
                permission: "!kick",
                description: "Kicks a player"));

            // BAN
            Command.TryRegister(SmartParse.CreateCommand(
                name: "ban",
                argTypes: new[] { SmartParse.UnimmunePlayer, SmartParse.OptionalGreedyString },
                action: delegate (Entity sender, object[] args)
                {
                    var target = args[0] as Entity;

                    if (args[1] is string str)
                        Funcs.Ban(target, sender.GetFormattedName(), str);
                    else
                        Funcs.Ban(target, sender.GetFormattedName());
                },
                usage: "!ban <player> [reason]",
                permission: "!ban",
                description: "Bans a player"));

            // TMPBAN
            Command.TryRegister(SmartParse.CreateCommand(
                name: "tmpban",
                argTypes: new[] { SmartParse.UnimmunePlayer, Parse.TimeSpan.Obj, SmartParse.OptionalGreedyString },
                action: delegate (Entity sender, object[] args)
                {
                    var target = args[0] as Entity;
                    if (args[1] is TimeSpan ts)
                    {
                        if (args[2] is string str)
                            Funcs.TempBan(target, sender.GetFormattedName(), ts, str);
                        else
                            Funcs.TempBan(target, sender.GetFormattedName(), ts);
                    }
                    else
                    {
                        if (args[2] is string str)
                            Funcs.TempBan(target, sender.GetFormattedName(), str);
                        else
                            Funcs.TempBan(target, sender.GetFormattedName());
                    }
                },
                usage: "!kick <player> [reason]",
                permission: "!kick",
                description: "Kicks a player"));

            #endregion
        }

        [EntryPoint]
        private static void Init()
        {
        }
    }
}

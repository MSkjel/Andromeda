using Andromeda;
using Andromeda.Events;
using Andromeda.Parse;
using InfinityScript;
using InfinityScript.PBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace BaseAdmin
{
    [Plugin]
    public static class Main
    {
        internal static SQLiteConnection Connection;

        private const string dateFormat = "yyyy-MM-dd HH:mm:ss";
        internal static string FormatDate(DateTime dateTime)
            => dateTime.ToString(dateFormat, System.Globalization.CultureInfo.InvariantCulture);

        internal static DateTime ParseDate(string str)
            => DateTime.ParseExact(str, dateFormat, System.Globalization.CultureInfo.InvariantCulture);

        static Main()
        {
            var file = GSCFunctions.GetDvar("database_path");

            GSCFunctions.SetDvarIfUninitialized("admindb_path", file);

            file = GSCFunctions.GetDvar("admindb_path");

            Connection = new SQLiteConnection($"Data Source={file};Version=3;");

            lock (Connection)
            {
                Connection.Open();

                using (var prepare = new SQLiteCommand("CREATE TABLE IF NOT EXISTS warnings (hwid VARCHAR(32) PRIMARY KEY NOT NULL, amount INTEGER NOT NULL);", Connection))
                {
                    prepare.ExecuteNonQuery();
                }

                using (var prepare = new SQLiteCommand("CREATE TABLE IF NOT EXISTS bans (banid INTEGER PRIMARY KEY NOT NULL, hwid VARCHAR(32) NOT NULL, guid BIGINT NOT NULL, name TEXT NOT NULL, issuer TEXT NOT NULL, reason TEXT NOT NULL, expire TEXT NOT NULL);", Connection))
                {
                    prepare.ExecuteNonQuery();
                }

                using (var prepare = new SQLiteCommand("DELETE FROM bans WHERE datetime('now', 'localtime') > datetime(expire);", Connection))
                {
                    prepare.ExecuteNonQuery();
                }
            }

            Common.Register(Admin.Instance);
        }

        [EntryPoint]
        private static void Init()
        {
            #region Commands
            #region Map
            // MAP
            Command.TryRegister(SmartParse.CreateCommand(
                name: "map",
                argTypes: new[] { Parse.GameMap.Obj },
                action: delegate (IClient sender, object[] args)
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
                action: delegate (IClient sender, object[] args)
                {
                    var msgs = "%iAvailable maps:".Yield()
                        .Concat(
                            Utils.Maps.Where(x => Utils.MapFilesExist(x.RawName))
                            .Select(x => $"%a{x.NiceName}%n")
                            .Condense());

                    sender.Tell(msgs);
                },
                usage: "!maps",
                permission: "maps",
                description: "Displays the available maps"));

            // SETNEXTMAP
            Command.TryRegister(SmartParse.CreateCommand(
                name: "setnextmap",
                argTypes: new[] { Parse.GameMap.Obj },
                action: delegate (IClient sender, object[] args)
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
                action: delegate (IClient sender, object[] args)
                {
                    string dsr = DSR.GetFullDSRName(args[0] as string);

                    DSR.SetNextMode(dsr);
                    Common.SayAll($"Mode has been changed to %h1{dsr} %nby %p{sender.GetFormattedName()}");
                    Utilities.ExecuteCommand("map_rotate");

                },
                usage: "!mode <mode>",
                permission: "mode",
                description: "Changes the mode to the mode specified"));

            // GAMETYPE
            Command.TryRegister(SmartParse.CreateCommand(
                name: "gametype",
                argTypes: new[] { Parse.GameMap.Obj, Parse.GameMode.Obj },
                action: delegate (Entity sender, object[] args)
                {
                    var map = args[0] as GameMap;
                    var dsr = DSR.GetFullDSRName(args[1] as string);

                    DSR.SetNextMapRotation(map.RawName, dsr);
                    Common.SayAll($"Map and mode have been changed to %h1{map.NiceName}%n, %h2{dsr} %nby %p{sender.GetFormattedName()}");
                    Utilities.ExecuteCommand("map_rotate");
                },
                usage: "!gametype <map> <mode>",
                permission: "gametype",
                description: "Changes the map and mode to the ones specified"));
            #endregion

            #region Admin

            // WARN
            Command.TryRegister(SmartParse.CreateCommand(
                name: "warn",
                argTypes: new[] { SmartParse.UnimmunePlayer, SmartParse.OptionalGreedyString },
                action: delegate (IClient sender, object[] args)
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
                action: delegate (IClient sender, object[] args)
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

            // RESETWARNINGS
            Command.TryRegister(SmartParse.CreateCommand(
                name: "resetwarnings",
                argTypes: new[] { SmartParse.UnimmunePlayer, SmartParse.OptionalGreedyString },
                action: delegate (IClient sender, object[] args)
                {
                    var target = args[0] as Entity;

                    if (args[1] is string str)
                        Funcs.ResetWarnings(target, sender.GetFormattedName(), str);
                    else
                        Funcs.ResetWarnings(target, sender.GetFormattedName());
                },
                usage: "!resetwarnings <player> [reason]",
                permission: "!resetwarnings",
                aliases: new[] { "resetwarns" },
                description: "Resets a player's warnings"));

            // KICK
            Command.TryRegister(SmartParse.CreateCommand(
                name: "kick",
                argTypes: new[] { SmartParse.UnimmunePlayer, SmartParse.OptionalGreedyString },
                action: delegate (IClient sender, object[] args)
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
                action: delegate (IClient sender, object[] args)
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
                action: delegate (IClient sender, object[] args)
                {
                    var target = args[0] as Entity;

                    if (args[1] is TimeSpan ts)
                    {
                        if (args[2] is string str)
                            Common.Admin.TempBan(target, sender.GetFormattedName(), ts, str);
                        else
                            Common.Admin.TempBan(target, sender.GetFormattedName(), ts);
                    }
                    else
                    {
                        if (args[2] is string str)
                            Common.Admin.TempBan(target, sender.GetFormattedName(), str);
                        else
                            Common.Admin.TempBan(target, sender.GetFormattedName());
                    }
                },
                usage: "!tmpban <player> [time] [reason]",
                permission: "!tmpban",
                description: "Temporarily bans a player"));

            // EXECUTECOMMAND
            Command.TryRegister(SmartParse.CreateCommand(
                name: "executecommand",
                argTypes: new[] { SmartParse.GreedyString },
                action: delegate (IClient sender, object[] args)
                {
                    var cmd = args[0] as string;

                    Utilities.ExecuteCommand(cmd);

                    sender.Tell("%aCommand executed.");
                },
                usage: "!executecommand <cmd>",
                aliases: new[] { "exec" },
                permission: "executecommand",
                description: "Executes a command in the server console"));

            // SEARCHBANS
            Command.TryRegister(SmartParse.CreateCommand(
                name: "searchbans",
                argTypes: new[] { SmartParse.GreedyString },
                action: delegate (IClient sender, object[] args)
                {
                    var filter = args[0] as string;

                    IEnumerator routine()
                    {
                        var cmd = new SQLiteCommand("SELECT * FROM bans WHERE (name LIKE @pattern OR hwid LIKE @pattern) ORDER BY banid DESC LIMIT 10;", Connection);

                        cmd.Parameters.AddWithValue("@pattern", $"%{filter}%");

                        yield return Async.Detach();

                        List<string> messages = new List<string>();
                        lock (Connection)
                        {
                            var reader = cmd.ExecuteReader();

                            while (reader.Read())
                                messages.Add($"%h1{reader["banid"]}%n - %p{reader["name"]}%n, {reader["hwid"]}");

                            reader.Close();
                        }

                        yield return Async.Attach();

                        sender.Tell("%aFound ban entries:".Yield().Concat(messages));
                    }

                    Async.Start(routine());

                },
                usage: "!searchbans <filter>",
                permission: "searchbans",
                description: "Searches the bans table"));

            // LASTBANS
            Command.TryRegister(SmartParse.CreateCommand(
                name: "lastbans",
                argTypes: new[] { SmartParse.OptionalRangedIntegerWithDefault(1, 10, 4) },
                action: delegate (IClient sender, object[] args)
                {
                    IEnumerator routine()
                    {
                        var cmd = new SQLiteCommand("SELECT * FROM bans ORDER BY banid DESC LIMIT 10;", Connection);

                        yield return Async.Detach();

                        List<string> messages = new List<string>();
                        lock (Connection)
                        {
                            var reader = cmd.ExecuteReader();

                            while (reader.Read())
                                messages.Add($"%h1{reader["banid"]}%n - %p{reader["name"]}%n, {reader["hwid"]}");

                            reader.Close();
                        }

                        yield return Async.Attach();

                        sender.Tell("%aFound ban entries:".Yield().Concat(messages));
                    }

                    Async.Start(routine());
                },
                usage: "!lastbans <1-10>",
                permission: "lastbans",
                description: "Shows the last given amount of bans"));

            // BANINFO
            Command.TryRegister(SmartParse.CreateCommand(
                name: "baninfo",
                argTypes: new[] { SmartParse.Integer },
                action: delegate (IClient sender, object[] args)
                {
                    var banid = (int)args[0];

                    IEnumerator routine()
                    {
                        var cmd = new SQLiteCommand("SELECT * FROM bans WHERE banid = @banid;", Connection);

                        cmd.Parameters.AddWithValue("@banid", banid);

                        yield return Async.Detach();

                        string[] messages = null;
                        lock(Connection)
                        {
                            var reader = cmd.ExecuteReader();

                            if(reader.Read())
                            {
                                messages = new[]
                                {
                                    $"Ban ID: %h1{reader["banid"]}",
                                    $"Name: %p{reader["name"]}",
                                    $"HWID: %i{reader["hwid"]}",
                                    $"GUID: %i{reader["guid"]}",
                                    $"Issuer: %p{reader["issuer"]}",
                                    $"Expiry: %a{reader["expire"]}",
                                    $"Reason: %i{reader["reason"]}",
                                };
                            }

                            reader.Close();
                        }

                        yield return Async.Attach();

                        if(messages == null)
                        {
                            sender.Tell($"%eEntry {banid} was not found.");
                            yield break;
                        }

                        sender.Tell(messages);
                        yield break;
                    }

                    Async.Start(routine());
                },
                usage: "!baninfo <banid>",
                permission: "baninfo",
                description: "Shows details of a ban"));

            // UNBAN
            Command.TryRegister(SmartParse.CreateCommand(
                name: "unban",
                argTypes: new[] { SmartParse.Integer },
                action: delegate (IClient sender, object[] args)
                {
                    var banid = (int)args[0];

                    IEnumerator routine()
                    {
                        var cmd = new SQLiteCommand("DELETE FROM bans WHERE banid = @banid;", Connection);

                        cmd.Parameters.AddWithValue("@banid", banid);

                        yield return Async.Detach();

                        int ret;
                        lock (Connection)
                        {
                            ret = cmd.ExecuteNonQuery();
                        }

                        yield return Async.Attach();

                        sender.Tell($"Return value: %i{ret}");
                    }

                    Async.Start(routine());
                },
                usage: "!unban <banid>",
                permission: "unban",
                description: "Removes a ban entry"));

            #endregion

            #region Balance

            // BALANCE
            Command.TryRegister(SmartParse.CreateCommand(
                name: "balance",
                argTypes: null,
                action: delegate (IClient sender, object[] args)
                {
                    Utils.DeadBalance(sender.GetFormattedName());
                },
                usage: "!balance",
                permission: "balance",
                description: "Balances teams using dead players"));

            // FORCEBALANCE
            Command.TryRegister(SmartParse.CreateCommand(
                name: "forcebalance",
                argTypes: null,
                action: delegate (IClient sender, object[] args)
                {
                    Utils.ForceBalance(sender.GetFormattedName());
                },
                usage: "!forcebalance",
                permission: "forcebalance",
                description: "Forces team balance"));

            // AUTOBALANCE
            Command.TryRegister(SmartParse.CreateCommand(
                name: "autobalance",
                argTypes: new[] { SmartParse.Boolean },
                action: delegate (IClient sender, object[] args)
                {
                    bool state = (bool)args[0];
                    GSCFunctions.SetDvar("autobalance", state ? 1 : 0);

                    Common.SayAll($"%p{sender.GetFormattedName()} %nhas {(state ? "^2enabled" : "^1disabled")} autobalance.");
                },
                usage: "!autobalance <1/0>",
                permission: "autobalance",
                description: "Enables or disables autobalance"));

            // AFK
            Command.TryRegister(SmartParse.CreateCommand(
                name: "afk",
                argTypes: null,
                action: delegate (Entity sender, object[] args)
                {
                    Utils.SetTeam(sender, "spectator");
                },
                usage: "!afk",
                description: "Sets your team to spectator"));

            // SETAFK
            Command.TryRegister(SmartParse.CreateCommand(
                name: "setafk",
                argTypes: new[] { SmartParse.Player },
                action: delegate (IClient sender, object[] args)
                {
                    var target = args[0] as Entity;

                    Utils.SetTeam(target, "spectator");
                    target.Tell($"You have been set to spectator by %p{sender.GetFormattedName()}%n.");
                },
                usage: "!setafk <player>",
                permission: "setafk",
                description: "Sets a player's team to spectator"));

            // RESTART
            Command.TryRegister(SmartParse.CreateCommand(
                name: "restart",
                argTypes: new[] { SmartParse.OptionalBoolean },
                action: delegate (IClient sender, object[] args)
                {
                    Common.SayAll($"Map has been restarted by %p{sender.GetFormattedName()}%n.");

                    if (args[0] is bool persistent)
                        GSCFunctions.Map_Restart(persistent);
                    else
                        Utilities.ExecuteCommand("fast_restart");
                },
                usage: "!restart <persistent>",
                aliases: new[] { "res" },
                permission: "restart",
                description: "Fast restarts the map"));

            // RESTART
            Command.TryRegister(SmartParse.CreateCommand(
                name: "end",
                argTypes: null,
                action: delegate (Entity sender, object[] args)
                {
                    Common.SayAll($"Game has been ended by %p{sender.GetFormattedName()}%n.");

                    foreach (Entity ent in BaseScript.Players)
                        ent.Notify("menuresponse", "menu", "endround");
                },
                usage: "!end",
                permission: "end",
                description: "Ends the game"));

            #endregion
            #endregion

            Script.PlayerConnected.Add((sender, player) =>
            {
                IEnumerator routine()
                {
                    var cmd = new SQLiteCommand("SELECT * FROM bans WHERE ((hwid = @hwid OR guid = @guid) AND (datetime(expire) > datetime('now', 'localtime') OR expire = 'permanent'));", Connection);

                    cmd.Parameters.AddWithValue("@hwid", player.HWID);
                    cmd.Parameters.AddWithValue("@guid", player.GUID);

                    bool found = false;
                    TimeSpan? timeSpan = null;
                    string message = null, issuer = null;
                    lock (Connection)
                    {
                        var reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            found = true;

                            if (reader["expire"] as string == "permanent")
                                timeSpan = null;
                            else
                                timeSpan = ParseDate(reader["expire"] as string) - DateTime.Now;

                            message = reader["reason"] as string;
                            issuer = reader["issuer"] as string;
                        }

                        reader.Close();
                    }

                    yield return Async.Attach();

                    if (found)
                    {
                        if (timeSpan.HasValue)
                        {
                            Funcs.TempBanKick(player, issuer, timeSpan.Value, message);
                            yield break;
                        }

                        Funcs.BanKick(player, issuer, message);
                        yield break;
                    }
                }

                Async.Start(routine());
            });

            GSCFunctions.SetDvarIfUninitialized("autobalance", 0);

            Script.PlayerKilled.Add((sender, args) =>
            {
                if (GSCFunctions.GetDvar("autobalance") != "0")
                {
                    Utils.CountPlayers(out int axis, out int allies, out _, out _);

                    switch (args.Player.SessionTeam)
                    {
                        case "axis" when (axis - allies > 1):
                            Utils.SetTeam(args.Player, "allies");
                            Common.SayAll($"%p{args.Player.Name} %nhas been automatically balanced.");
                            return;
                        case "allies" when (allies - axis > 1):
                            Common.SayAll($"%p{args.Player.Name} %nhas been automatically balanced.");
                            Utils.SetTeam(args.Player, "axis");
                            return;
                    }
                }
            });
        }
    }
}

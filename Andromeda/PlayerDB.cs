#define DBWorkaround

using InfinityScript;
using InfinityScript.Events;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Andromeda
{
    [Plugin]
    public static class PlayerDB
    {
        public static readonly Event<Entity> PlayerLoggedIn = new Event<Entity>(Events.Events.ErrorHandler(nameof(PlayerLoggedIn)));
        public static readonly Event<Entity> PlayerLoggedOut = new Event<Entity>(Events.Events.ErrorHandler(nameof(PlayerLoggedOut)));

        public static string GetDBFieldOr(this Entity ent, string field, string def = default)
        {
            if (TryGetInfo(ent, out var info))
                if (info.LoggedIn && info.Data.TryGetValue(field, out var ret))
                    return ret;

            return def;
        }

        public static bool TrySetDBField(this Entity ent, string field, string value)
        {
            if (TryGetInfo(ent, out var info) && info.LoggedIn)
            {
                info.Data[field] = value;
#if DBWorkaround
                info.UpdateDataAsync();
#endif
                return true;
            }

            return false;
        }

        public static bool TryRemoveDBField(this Entity ent, string field)
        {
            if (TryGetInfo(ent, out var info) && info.LoggedIn)
            {
                info.Data.Remove(field);
#if DBWorkaround
                info.UpdateDataAsync();
#endif
                return true;
            }

            return false;
        }

        internal static bool TryGetInfo(this Entity ent, out PlayerInfo row)
        {
            var possibleRow = ConnectedPlayers[ent.EntRef];

            if (possibleRow != null)
            {
                row = possibleRow;
                return true;
            }

            row = default;
            return false;
        }

        public static bool IsLogged(this Entity ent)
        {
            if (TryGetInfo(ent, out var info))
                return info.LoggedIn;

            return false;
        }

        private const string dateFormat = "yyyy-MM-dd HH:mm:ss";
        internal static string FormatDate(DateTime dateTime)
            => dateTime.ToString(dateFormat, System.Globalization.CultureInfo.InvariantCulture);

        internal class PlayerInfo
        {
            public string HWID;
            public byte[] PasswordHash;
            public Dictionary<string, string> Data;

            public bool LoggedIn { get; set; }

            public static PlayerInfo Get(SQLiteDataReader reader)
            {
                if (reader.Read())
                {
                    return new PlayerInfo
                    {
                        HWID = reader["hwid"] as string,
                        PasswordHash = reader["password"] as byte[],
                        Data = JsonConvert.DeserializeObject<Dictionary<string, string>>(reader["data"] as string),
                        LoggedIn = false,
                    };
                }

                return null;
            }

            public void UpdateDataAsync(Action<int> action = null)
            {
                IEnumerator routine()
                {
                    var cmd = new SQLiteCommand("UPDATE players SET data = @data WHERE hwid = @hwid", Connection);

                    cmd.Parameters.AddWithValue("@data", JsonConvert.SerializeObject(Data));
                    cmd.Parameters.AddWithValue("@hwid", HWID);

                    yield return Async.Detach();

                    lock (Connection)
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                Async.Start(routine());
            }

            public void UpdateData()
            {
                var cmd = new SQLiteCommand("UPDATE players SET data = @data WHERE hwid = @hwid", Connection);

                cmd.Parameters.AddWithValue("@data", JsonConvert.SerializeObject(Data));
                cmd.Parameters.AddWithValue("@hwid", HWID);

                lock(Connection)
                {
                    cmd.ExecuteNonQuery();
                }
            }

            public override string ToString()
                => $"Entry({HWID},{BitConverter.ToString(PasswordHash)})";
        }

        private static byte[] GetLoginHash(Entity player)
            => Md5(player.HWID + player.GUID.ToString() + player.IP.Address);

        private static byte[] Sha256(string str)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                return sha256Hash.ComputeHash(Encoding.ASCII.GetBytes(str));
            }
        }

        private static byte[] Md5(string str)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                return md5Hash.ComputeHash(Encoding.ASCII.GetBytes(str));
            }
        }

        private static bool ByteArrayEquals(byte[] hash1, byte[] hash2)
        {
            if (hash1.Length != hash2.Length)
                return false;

            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i])
                    return false;
            }

            return true;
        }

        internal static SQLiteConnection Connection;
        internal static PlayerInfo[] ConnectedPlayers = new PlayerInfo[18];

        internal static bool TryRegister(Entity ent, string password, Action onFinish)
        {
            var exists = ConnectedPlayers[ent.EntRef] != null;

            if (!exists)
            {
                IEnumerator routine()
                {
                    var hwid = ent.HWID;
                    byte[] bytes = Sha256(password);

                    var command = new SQLiteCommand("INSERT INTO players (hwid, password, data) VALUES (@hwid, @password, @data);", Connection);

                    command.Parameters.AddWithValue("@hwid", hwid);
                    command.Parameters.AddWithValue("@password", bytes);
                    command.Parameters.AddWithValue("@data", "{}");

                    yield return Async.Detach();

                    lock (Connection)
                    {
                        command.ExecuteNonQuery();
                    }

                    yield return Async.Attach();

                    Log.Info($"Registered HWID {hwid} inside database");

                    ConnectedPlayers[ent.EntRef] = new PlayerInfo
                    {
                        HWID = hwid,
                        PasswordHash = bytes,
                        Data = new Dictionary<string, string>(),
                        LoggedIn = true,
                    };

                    onFinish();

                    yield return UpdateLogin(GetLoginHash(ent));

                }

                Async.Start(routine());
            }

            return !exists;
        }

        private static IEnumerator UpdateLogin(byte[] hash)
        {
            var updateLogged = new SQLiteCommand("INSERT OR REPLACE INTO loggedin (hash, time) VALUES (@hash, @time);", Connection);

            updateLogged.Parameters.AddWithValue("@time", FormatDate(DateTime.Now + TimeSpan.FromHours(6)));
            updateLogged.Parameters.AddWithValue("@hash", hash);

            yield return Async.Detach();

            lock (Connection)
            {
                updateLogged.ExecuteNonQuery();
            }
        }

        [EntryPoint]
        private static void Init()
        {
            Script.PlayerConnected.Add((sender, player) =>
            {
                IEnumerator routine()
                {
                    var cmd = new SQLiteCommand($"SELECT * FROM players WHERE hwid = @value;", Connection);

                    cmd.Parameters.AddWithValue("@value", player.HWID);

                    yield return Async.Detach();

                    PlayerInfo found;
                    lock (Connection)
                    {
                        found = PlayerInfo.Get(cmd.ExecuteReader());
                    }

                    yield return Async.Attach();

                    ConnectedPlayers[player.EntRef] = found;

                    if (found != null)
                        Log.Info($"Found entry for player {player.Name}");
                    else
                        Log.Info($"Did not find entry for player {player.Name}");

                    if (found != null)
                    {
                        var hash = GetLoginHash(player);

                        var findLogged = new SQLiteCommand("SELECT * FROM loggedin WHERE hash = @hash;", Connection);

                        findLogged.Parameters.AddWithValue("@hash", hash);

                        yield return Async.Detach();

                        bool logged;
                        DateTime time = DateTime.MinValue;
                        lock (Connection)
                        {
                            var reader = findLogged.ExecuteReader();

                            if (reader.Read())
                            {
                                time = DateTime.ParseExact(reader["time"] as string, dateFormat, System.Globalization.CultureInfo.InvariantCulture);

                                if (DateTime.Now < time)
                                    logged = true;
                                else
                                    logged = false;
                            }
                            else
                                logged = false;
                        }

                        yield return Async.Attach();

                        if (logged)
                        {
                            ConnectedPlayers[player.EntRef].LoggedIn = true;

                            player.Tell("%iYou have logged in automatically.");
                            Log.Info($"Player {player.Name} automatically logged in.");

                            PlayerLoggedIn.Run(null, player);

                            yield return UpdateLogin(hash);
                        }
                    }
                }

                Async.Start(routine());
            }, int.MinValue);

            Script.PlayerDisconnected.Add((sender, player) =>
            {
                if (player.IsLogged())
                    PlayerLoggedOut.Run(null, player);

                ConnectedPlayers[player.EntRef]?.UpdateDataAsync(delegate
                {
                    ConnectedPlayers[player.EntRef] = null;
                });
            }, int.MaxValue);

            #region Commands
            // REGISTER
            Command.TryRegister(Parse.SmartParse.CreateCommand(
                name: "register",
                argTypes: new[] { Parse.SmartParse.String, Parse.SmartParse.String },
                action: delegate (Entity sender, object[] args)
                {
                    var pw = args[0] as string;
                    var confirmation = args[1] as string;

                    if (pw != confirmation)
                    {
                        sender.Tell("%ePasswords do not match.");
                        return;
                    }

                    if (!TryRegister(sender, pw, () =>
                    {
                        sender.Tell("%iYou have successfully registered.");

                        ConnectedPlayers[sender.EntRef].LoggedIn = true;
                    }))
                    {
                        sender.Tell("%eYou are already registered. Login instead.");
                        return;
                    }


                },
                usage: "!register <password> <confirm>",
                description: "Register to the server's database"));

            // LOGIN
            Command.TryRegister(Parse.SmartParse.CreateCommand(
                name: "login",
                argTypes: new[] { Parse.SmartParse.String },
                action: delegate (Entity sender, object[] args)
                {
                    var pw = args[0] as string;

                    if (sender.TryGetInfo(out var row))
                    {
                        if (row.LoggedIn)
                        {
                            sender.Tell("%eYou are already logged in.");
                            return;
                        }

                        if (!ByteArrayEquals(row.PasswordHash, Sha256(pw)))
                        {
                            sender.Tell("%eIncorrect password!");
                            return;
                        }

                        row.LoggedIn = true;

                        Async.Start(UpdateLogin(GetLoginHash(sender)));

                        PlayerLoggedIn.Run(null, sender);

                        sender.Tell("%iYou have logged in.");
                        return;
                    }

                    sender.Tell("%eYou are not registered!");
                    return;
                },
                usage: "!login <password>",
                description: "Logs you into the server database"));

            // LOGOUT
            Command.TryRegister(Parse.SmartParse.CreateCommand(
                name: "logout",
                argTypes: new Parse.IArgParse[0],
                action: delegate (Entity sender, object[] args)
                {
                    if (sender.TryGetInfo(out var row))
                    {
                        if (!row.LoggedIn)
                        {
                            sender.Tell("%eYou aren't logged in.");
                            return;
                        }

                        row.LoggedIn = false;

                        IEnumerator routine()
                        {
                            var cmd = new SQLiteCommand("DELETE FROM loggedin WHERE hash = @hash;", Connection);

                            cmd.Parameters.AddWithValue("@hash", GetLoginHash(sender));

                            yield return Async.Detach();

                            lock (Connection)
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }

                        Async.Start(routine());

                        PlayerLoggedOut.Run(null, sender);

                        sender.Tell("%iYou have logged out.");
                        return;
                    }
                },
                usage: "!logout",
                description: "Logs you out of the server database"));

            // CHANGEPASSWORD
            Command.TryRegister(Parse.SmartParse.CreateCommand(
                name: "changepassword",
                argTypes: new[] { Parse.SmartParse.String, Parse.SmartParse.String },
                action: delegate (Entity sender, object[] args)
                {
                    var pw = args[0] as string;
                    var confirmation = args[1] as string;

                    if (sender.TryGetInfo(out var row) && row.LoggedIn)
                    {
                        if (pw != confirmation)
                        {
                            sender.Tell("%ePasswords do not match.");
                            return;
                        }

                        IEnumerator routine()
                        {
                            var hash = Sha256(pw);
                            var cmd = new SQLiteCommand("UPDATE players SET password = @hash WHERE hwid = @value;", Connection);

                            cmd.Parameters.AddWithValue("@value", sender.HWID);
                            cmd.Parameters.AddWithValue("@hash", hash);

                            yield return Async.Detach();

                            int status;
                            lock (Connection)
                            {
                                status = cmd.ExecuteNonQuery();
                            }

                            yield return Async.Attach();

                            if (status == -1)
                            {
                                sender.Tell($"%eError changing password: {status}");
                                yield break;
                            }

                            row.PasswordHash = hash;
                            sender.Tell("%iPassword changed successfully.");
                        }

                        Async.Start(routine());
                    }
                    else
                        sender.Tell("%ePlease log in to change password.");
                },
                usage: "!changepassword <newpassword> <confirm>",
                description: "Logs you into the server database"));

            #endregion
        }

        [Cleanup]
        private static void Cleanup()
        {
            lock (Connection)
            {
                foreach (var info in ConnectedPlayers)
                {
                    if (info != null)
                        info.UpdateData();
                }

                Connection.Close();
            }
        }

        static PlayerDB()
        {
            System.IO.Directory.CreateDirectory(@"scripts\Andromeda");

            Connection = new SQLiteConnection(@"Data Source=scripts\Andromeda\players.sqlite;Version=3;");

            lock (Connection)
            {
                Log.Info("Preparing DB...");

                Connection.Open();

                using (var prepare = new SQLiteCommand("CREATE TABLE IF NOT EXISTS players (hwid VARCHAR(32) PRIMARY KEY NOT NULL, password BLOB NOT NULL, data LONGTEXT);", Connection))
                {
                    Log.Info($"result: {prepare.ExecuteNonQuery()}");
                }

                using (var prepare = new SQLiteCommand("CREATE TABLE IF NOT EXISTS loggedin (hash BLOB PRIMARY KEY NOT NULL, time TEXT);", Connection))
                {
                    Log.Info($"result: {prepare.ExecuteNonQuery()}");
                }

                using (var prepare = new SQLiteCommand("DELETE FROM loggedin WHERE time < datetime('now');", Connection))
                {
                    Log.Info($"result: {prepare.ExecuteNonQuery()}");
                }

                Log.Info("Done preparing.");
            }
        }
    }
}

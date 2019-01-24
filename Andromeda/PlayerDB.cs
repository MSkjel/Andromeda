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

        private const string dateFormat = "yyyy-MM-dd HH:mm:ss.fffffff";

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

            public void UpdateData(Action<int> action = null)
            {
                var cmd = new SQLiteCommand("UPDATE players SET data = @data WHERE hwid = @hwid");

                cmd.Parameters.AddWithValue("@data", JsonConvert.SerializeObject(Data));
                cmd.Parameters.AddWithValue("@hwid", HWID);

                ExecuteNonQuery(cmd, action);
            }

            public override string ToString()
                => $"Entry({HWID},{BitConverter.ToString(PasswordHash)})";
        }

        private static byte[] GetLoginHash(Entity player)
            => Sha256(player.HWID + player.GUID.ToString());

        private static byte[] Sha256(string str)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                return sha256Hash.ComputeHash(Encoding.ASCII.GetBytes(str));
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

        internal static void ExecuteNonQuery(SQLiteCommand cmd, Action<int> callback = null)
        {
            IEnumerator pseudoThread()
            {
                yield return Async.Detach();

                int ret;
                lock (Connection)
                {
                    cmd.Connection = Connection;
                    ret = cmd.ExecuteNonQuery();

                    cmd.Dispose();
                }

                yield return Async.Attach();

                callback?.Invoke(ret);
            }

            Async.Start(pseudoThread());
        }

        internal static void ExecuteReader<T>(SQLiteCommand cmd, Func<SQLiteDataReader, T> reading, Action<T> callback)
        {
            IEnumerator pseudoThread()
            {
                yield return Async.Detach();

                T ret;

                lock (Connection)
                {
                    cmd.Connection = Connection;
                    var reader = cmd.ExecuteReader();

                    ret = reading(reader);

                    cmd.Dispose();
                }

                yield return Async.Attach();

                callback(ret);
            }

            Async.Start(pseudoThread());
        }

        internal static bool TryRegister(Entity ent, string password, Action onFinish)
        {
            var exists = ConnectedPlayers[ent.EntRef] != null;
            var hwid = ent.HWID;

            if (!exists)
            {
                byte[] bytes = Sha256(password);

                var command = new SQLiteCommand("INSERT INTO players (hwid, password, data) VALUES (@hwid, @password, @data);");

                command.Parameters.AddWithValue("@hwid", hwid);
                command.Parameters.AddWithValue("@password", bytes);
                command.Parameters.AddWithValue("@data", "{}");

                ExecuteNonQuery(command, delegate
                {
                    Log.Info($"Registered HWID {hwid} inside database");

                    ConnectedPlayers[ent.EntRef] = new PlayerInfo
                    {
                        HWID = hwid,
                        PasswordHash = bytes,
                        Data = new Dictionary<string, string>(),
                        LoggedIn = true,
                    };

                    onFinish();
                });
            }

            return !exists;
        }

        [EntryPoint]
        private static void Init()
        {
            Script.PlayerConnected.Add((sender, player) =>
            {
                var cmd = new SQLiteCommand($"SELECT * FROM players WHERE hwid = @value;");

                cmd.Parameters.AddWithValue("@value", player.HWID);

                ExecuteReader(cmd, PlayerInfo.Get,
                delegate (PlayerInfo entry)
                {
                    ConnectedPlayers[player.EntRef] = entry;

                    if (entry != null)
                        Log.Info($"Found entry for player {player.Name}");
                    else
                        Log.Info($"Did not find entry for player {player.Name}");

                    if (entry != null)
                    {
                        var hash = Sha256(player.HWID + player.GUID.ToString());

                        var findLogged = new SQLiteCommand("SELECT * FROM loggedin WHERE hash = @hash;");

                        findLogged.Parameters.AddWithValue("@hash", hash);

                        ExecuteReader(findLogged, delegate (SQLiteDataReader reader)
                        {
                            if (reader.Read())
                            {
                                var time = DateTime.ParseExact(reader["time"] as string, dateFormat, System.Globalization.CultureInfo.InvariantCulture);

                                if (DateTime.Now < time)
                                    return true;
                            }

                            return false;
                        },
                        delegate (bool logged)
                        {
                            if (logged)
                            {
                                ConnectedPlayers[player.EntRef].LoggedIn = true;
                                PlayerLoggedIn.Run(null, player);

                                var updateLogged = new SQLiteCommand("UPDATE loggedin SET time = @time WHERE hash = @hash;");

                                updateLogged.Parameters.AddWithValue("@time", DateTime.Now + TimeSpan.FromHours(6));
                                updateLogged.Parameters.AddWithValue("@hash", hash);

                                ExecuteNonQuery(cmd);
                            }
                        });
                    }
                });
            }, int.MinValue);

            Script.PlayerDisconnected.Add((sender, player) =>
            {
                if (player.IsLogged())
                    PlayerLoggedOut.Run(null, player);

                ConnectedPlayers[player.EntRef]?.UpdateData(delegate
                {
                    ConnectedPlayers[player.EntRef] = null;
                });
            }, int.MaxValue);

            #region Commands
            // REGISTER
            Command.TryRegister(Cmd.SmartParse.CreateCommand(
                name: "register",
                argTypes: new[] { Cmd.SmartParse.String, Cmd.SmartParse.String },
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
            Command.TryRegister(Cmd.SmartParse.CreateCommand(
                name: "login",
                argTypes: new[] { Cmd.SmartParse.String },
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

                        var cmd = new SQLiteCommand("INSERT INTO loggedin (hash, time) VALUES (@hash, @time);");

                        cmd.Parameters.AddWithValue("@hash", GetLoginHash(sender));
                        cmd.Parameters.AddWithValue("@time", DateTime.Now + TimeSpan.FromHours(6));

                        ExecuteNonQuery(cmd);

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
            Command.TryRegister(Cmd.SmartParse.CreateCommand(
                name: "logout",
                argTypes: new Cmd.IArgParse[0],
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

                        var cmd = new SQLiteCommand("DELETE FROM loggedin WHERE hash = @hash;");

                        cmd.Parameters.AddWithValue("@hash", GetLoginHash(sender));

                        ExecuteNonQuery(cmd);

                        PlayerLoggedOut.Run(null, sender);

                        sender.Tell("%iYou have logged out.");
                        return;
                    }
                },
                usage: "!logout",
                description: "Logs you out of the server database"));

            // CHANGEPASSWORD
            Command.TryRegister(Cmd.SmartParse.CreateCommand(
                name: "changepassword",
                argTypes: new[] { Cmd.SmartParse.String, Cmd.SmartParse.String },
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

                        var hash = Sha256(pw);
                        var cmd = new SQLiteCommand("UPDATE players SET password = @hash WHERE hwid = @value;");

                        cmd.Parameters.AddWithValue("@value", sender.HWID);
                        cmd.Parameters.AddWithValue("@hash", hash);

                        ExecuteNonQuery(cmd, delegate (int status)
                        {
                            if (status == -1)
                            {
                                sender.Tell($"%eError changing password: {status}");
                                return;
                            }

                            row.PasswordHash = hash;
                            sender.Tell("%iPassword changed successfully.");
                        });
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
            foreach (var info in ConnectedPlayers)
            {
                info?.UpdateData();
            }

            lock (Connection)
            {
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

                using (var prepare = new SQLiteCommand("DELETE FROM loggedin WHERE time > datetime('now');", Connection))
                {
                    Log.Info($"result: {prepare.ExecuteNonQuery()}");
                }

                Log.Info("Done preparing.");
            }
        }
    }
}

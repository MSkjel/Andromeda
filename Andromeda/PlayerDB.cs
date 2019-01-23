using InfinityScript;
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
        internal class PlayerInfo
        {
            public string HWID;
            public byte[] PasswordHash;
            public string JsonString;

            public bool LoggedIn { get; set; }

            public static PlayerInfo Get(SQLiteDataReader reader)
            {
                if (reader.Read())
                {
                    return new PlayerInfo
                    {
                        HWID = reader["hwid"] as string,
                        PasswordHash = reader["password"] as byte[],
                        JsonString = reader["data"] as string,
                        LoggedIn = false,
                    };
                }

                return null;
            }

            public void UpdateData()
            {
                var cmd = new SQLiteCommand("UPDATE players SET data = @data WHERE hwid = @hwid");

                cmd.Parameters.AddWithValue("@data", JsonString);
                cmd.Parameters.AddWithValue("@hwid", HWID);

                ExecuteNonQuery(cmd);
            }

            public override string ToString()
                => $"Entry({HWID},{BitConverter.ToString(PasswordHash)})";
        }

        internal static bool TryGetRow(this Entity ent, out PlayerInfo row)
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
            if (TryGetRow(ent, out var row))
                return row.LoggedIn;

            return false;
        }

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

        public static void ExecuteNonQuery(SQLiteCommand cmd, Action<int> callback = null)
        {
            IEnumerator pseudoThread()
            {
                using (cmd)
                {
                    var task = Task.Factory.StartNew(delegate
                    {
                        lock (Connection)
                        {
                            cmd.Connection = Connection;
                            var ret = cmd.ExecuteNonQuery();

                            return ret;
                        }
                    });

                    while (!task.IsCompleted)
                        yield return BaseScript.WaitForFrame();

                    callback?.Invoke(task.Result);
                }
            }

            BaseScript.StartAsync(pseudoThread());
        }

        public static void ExecuteReader<T>(SQLiteCommand cmd, Func<SQLiteDataReader, T> reading, Action<T> callback)
        {
            IEnumerator pseudoThread()
            {
                using (cmd)
                {
                    Task<T> task;
                    lock (Connection)
                    {
                        cmd.Connection = Connection;
                        task = Task.Factory.StartNew(delegate
                        {
                            var reader = cmd.ExecuteReader();
                            var ret = reading(reader);
                            return ret;
                        });

                        while (!task.IsCompleted)
                            yield return BaseScript.WaitForFrame();
                    }

                    callback(task.Result);
                }
            }

            BaseScript.StartAsync(pseudoThread());
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
                        JsonString = "{}",
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
                });
            });

            Script.PlayerDisconnected.Add((sender, player) =>
            {
                ConnectedPlayers[player.EntRef] = null;
            });

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

                    if (sender.TryGetRow(out var row))
                    {
                        if(row.LoggedIn)
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
                    if (sender.TryGetRow(out var row))
                    {
                        if (!row.LoggedIn)
                        {
                            sender.Tell("%eYou aren't logged in.");
                            return;
                        }

                        row.LoggedIn = false;
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

                    if (sender.TryGetRow(out var row) && row.LoggedIn)
                    {
                        if (pw != confirmation)
                        {
                            sender.Tell("%ePasswords do not match.");
                            return;
                        }

                        var hash = Sha256(pw);
                        var cmd = new SQLiteCommand("UPDATE players SET password = @hash WHERE hwid = @value");

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
                    {
                        sender.Tell("%ePlease log in to change password.");
                        return;
                    }
                },
                usage: "!changepassword <newpassword> <confirm>",
                description: "Logs you into the server database"));

            #endregion

        }

        private static void Cleanup()
        {
            foreach(var info in ConnectedPlayers)
            {
                info?.UpdateData();
            }
        }

        static PlayerDB()
        {
            System.IO.Directory.CreateDirectory(@"scripts\Andromeda");

            Connection = new SQLiteConnection(@"Data Source=scripts\Andromeda\players.sqlite;Version=3;");

            Log.Info("Preparing DB...");

            Connection.Open();

            using (var prepare = new SQLiteCommand(@"CREATE TABLE IF NOT EXISTS players (hwid VARCHAR(32) PRIMARY KEY NOT NULL, password BLOB NOT NULL, data LONGTEXT);", Connection))
            {
                Log.Info($"result: {prepare.ExecuteNonQuery()}");
            }

            Log.Info("Done preparing.");
        }
    }
}

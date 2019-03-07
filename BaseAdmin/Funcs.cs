using Andromeda;
using InfinityScript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BaseAdmin
{
    public static class Funcs
    {
        private static Config Config => Config.Instance;

        private static void DelayedKick(Entity ent, string message)
        {
            var cmd = $"dropclient {ent.EntRef} \"{message}\"";

            BaseScript.AfterDelay(150, delegate
            {
                Utilities.ExecuteCommand(cmd);
            });
        }

        public static void Ban(Entity ent, string issuer, string message = "You have been banned")
        {
            IEnumerator routine()
            {
                var cmd = new SQLiteCommand("INSERT INTO bans (hwid, guid, name, expire, issuer, reason) VALUES (@hwid, @guid, @name, @time, @issuer, @reason);", Main.Connection);

                cmd.Parameters.AddWithValue("@hwid", ent.HWID);
                cmd.Parameters.AddWithValue("@name", ent.Name);
                cmd.Parameters.AddWithValue("@guid", ent.GUID);
                cmd.Parameters.AddWithValue("@issuer", issuer);
                cmd.Parameters.AddWithValue("@reason", message);
                cmd.Parameters.AddWithValue("@time", "permanent");

                BanKick(ent, issuer, message);

                Common.SayAll(Config.BanMessages.BanMessageServer.FormatServerMessage(ent, issuer, message));

                yield return Async.Detach();
                lock (Main.Connection)
                {
                    cmd.ExecuteNonQuery();
                }
            }

            Async.Start(routine());
        }

        internal static void BanKick(Entity ent, string issuer, string message)
            => DelayedKick(ent, Config.BanMessages.BanMessagePlayer.FormatServerMessage(ent, issuer, message).ColorFormat());

        public static void Kick(Entity ent, string issuer, string message = "You have been kicked")
        {
            DelayedKick(ent, Config.KickMessages.KickMessagePlayer.FormatServerMessage(ent, issuer, message).ColorFormat());
            Common.SayAll(Config.KickMessages.KickMessagePlayer.FormatServerMessage(ent, issuer, message));
        }

        public static void TempBan(Entity ent, string issuer, TimeSpan timeSpan, string message = "You have been temporarily banned")
        {
            IEnumerator routine()
            {
                var cmd = new SQLiteCommand("INSERT INTO bans (hwid, guid, name, expire, issuer, reason) VALUES (@hwid, @guid, @name, @time, @issuer, @reason);", Main.Connection);

                cmd.Parameters.AddWithValue("@hwid", ent.HWID);
                cmd.Parameters.AddWithValue("@guid", ent.GUID);
                cmd.Parameters.AddWithValue("@name", ent.Name);
                cmd.Parameters.AddWithValue("@issuer", issuer);
                cmd.Parameters.AddWithValue("@reason", message);
                cmd.Parameters.AddWithValue("@time", Main.FormatDate(DateTime.Now + timeSpan));

                TempBanKick(ent, issuer, timeSpan, message);

                var spanstr = $"{timeSpan.Days}d{timeSpan.Hours}h{timeSpan.Minutes}m";

                Common.SayAll(Config.TempBanMessages.TempBanMessagePlayer.FormatServerMessage(ent, issuer, message, spanstr));

                yield return Async.Detach();

                lock (Main.Connection)
                {
                    cmd.ExecuteNonQuery();
                }
            }

            Async.Start(routine());
        }

        internal static void TempBanKick(Entity ent, string issuer, TimeSpan timeSpan, string message)
        {
            var spanstr = $"{timeSpan.Days}d{timeSpan.Hours}h{timeSpan.Minutes}m";

            DelayedKick(ent, Config.TempBanMessages.TempBanMessagePlayer.FormatServerMessage(ent, issuer, message, spanstr).ColorFormat());
        }

        public static void Unwarn(Entity ent, string issuer, string reason = "You have been unwarned")
        {
            IEnumerator routine()
            {
                var cmd = new SQLiteCommand("SELECT amount FROM warnings WHERE hwid = @hwid;", Main.Connection);

                cmd.Parameters.AddWithValue("@hwid", ent.HWID);

                yield return Async.Detach();

                long amount = 0;
                lock (Main.Connection)
                {
                    var value = cmd.ExecuteScalar();

                    if (value != null)
                        amount = (long)value;
                }

                yield return Async.Attach();

                amount--;

                if (amount <= 0)
                    amount = 0;

                Common.SayAll(Config.Warns.UnwarnMessageServer.FormatServerMessage(ent, issuer, reason));
                ent.IPrintLnBold(Config.Warns.UnwarnMessagePlayer.FormatServerMessage(ent, issuer, reason).ColorFormat());

                var newcmd = new SQLiteCommand(Main.Connection);
                if (amount == 0)
                {
                    newcmd.CommandText = "DELETE FROM warnings WHERE hwid = @hwid;";

                    newcmd.Parameters.AddWithValue("@hwid", ent.HWID);
                }
                else
                {
                    newcmd.CommandText = "INSERT OR REPLACE INTO warnings (hwid, amount) VALUES (@hwid, @amount);";

                    newcmd.Parameters.AddWithValue("@hwid", ent.HWID);
                    newcmd.Parameters.AddWithValue("@amount", amount);
                }

                yield return Async.Detach();

                lock (Main.Connection)
                {
                    newcmd.ExecuteNonQuery();
                }
            }

            Async.Start(routine());
        }

        public static void ResetWarnings(Entity ent, string issuer, string reason = "You have been unwarned")
        {
            IEnumerator routine()
            {
                Common.SayAll($"%p{ent.GetFormattedName()} %nhad their warnings ^2reset %nby %p{issuer}%n. Reason: %i{reason}");

                var cmd = new SQLiteCommand("DELETE FROM warnings WHERE hwid = @hwid;", Main.Connection);

                cmd.Parameters.AddWithValue("@hwid", ent.HWID);

                yield return Async.Detach();

                lock (Main.Connection)
                {
                    cmd.ExecuteNonQuery();
                }
            }

            Async.Start(routine());
        }

        public static void Warn(Entity ent, string issuer, string reason = "You have been warned")
        {
            IEnumerator routine()
            {
                var cmd = new SQLiteCommand("SELECT amount FROM warnings WHERE hwid = @hwid;", Main.Connection);

                cmd.Parameters.AddWithValue("@hwid", ent.HWID);

                yield return Async.Detach();

                long amount = 0;
                lock (Main.Connection)
                {
                    var value = cmd.ExecuteScalar();

                    if (value != null)
                        amount = (long)value;
                }

                yield return Async.Attach();

                amount++;

                if (amount >= Config.Warns.MaxWarns)
                {
                    Common.SayAll(Config.Warns.WarnMessageServer.FormatServerMessage(ent, issuer, reason, "", (int)amount, Config.Warns.MaxWarns));
                    Common.Admin.TempBan(ent, issuer, reason);

                    amount = 0;
                }
                else
                {
                    Common.SayAll(Config.Warns.WarnMessageServer.FormatServerMessage(ent, issuer, reason, "", (int)amount, Config.Warns.MaxWarns));
                    ent.IPrintLnBold(Config.Warns.WarnMessagePlayer.FormatServerMessage(ent, issuer, reason, "", (int)amount, Config.Warns.MaxWarns).ColorFormat());
                }

                var newcmd = new SQLiteCommand(Main.Connection);

                if (amount == 0)
                    newcmd.CommandText = "DELETE FROM warnings WHERE hwid = @hwid;";
                else
                    newcmd.CommandText = "INSERT OR REPLACE INTO warnings (hwid, amount) VALUES (@hwid, @amount);";

                newcmd.Parameters.AddWithValue("@hwid", ent.HWID);
                newcmd.Parameters.AddWithValue("@amount", amount);

                yield return Async.Detach();

                lock (Main.Connection)
                {
                    newcmd.ExecuteNonQuery();
                }
            }

            Async.Start(routine());
        }
    }
}

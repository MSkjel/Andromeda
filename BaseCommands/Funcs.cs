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

        internal static readonly int MaxWarnings = 3;

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
                var cmd = new SQLiteCommand("INSERT INTO bans (hwid, guid, expire, issuer, reason) VALUES (@hwid, @guid, @time, @issuer, @reason);", Main.Connection);

                cmd.Parameters.AddWithValue("@hwid", ent.HWID);
                cmd.Parameters.AddWithValue("@guid", ent.GUID);
                cmd.Parameters.AddWithValue("@issuer", issuer);
                cmd.Parameters.AddWithValue("@reason", message);
                cmd.Parameters.AddWithValue("@time", "permanent");

                BanKick(ent, issuer, message);

                Common.SayAll($"%p{ent.GetFormattedName()} %nhas been ^1banned %nby %p{issuer}%n. Reason: %i{message}");

                yield return Async.Detach();
                lock (Main.Connection)
                {
                    cmd.ExecuteNonQuery();
                }
            }

            Async.Start(routine());
        }

        internal static void BanKick(Entity ent, string issuer, string message)
            => DelayedKick(ent, $"You are permanently banned by %p{issuer}%n. Reason: %i{message}".ColorFormat());

        public static void Kick(Entity ent, string issuer, string message = "You have been kicked")
        {
            DelayedKick(ent, $"Kicked by %p{issuer}%n. Reason: %i{message}\"".ColorFormat());

            Common.SayAll($"%p{ent.GetFormattedName()} %nhas been ^1kicked %nby %p{issuer}%n. Reason: %i{message}");
        }

        public static void TempBan(Entity ent, string issuer, TimeSpan timeSpan, string message = "You have been temporarily banned")
        {
            IEnumerator routine()
            {
                var cmd = new SQLiteCommand("INSERT INTO bans (hwid, guid, expire, issuer, reason) VALUES (@hwid, @guid, @time, @issuer, @reason);", Main.Connection);

                cmd.Parameters.AddWithValue("@hwid", ent.HWID);
                cmd.Parameters.AddWithValue("@guid", ent.GUID);
                cmd.Parameters.AddWithValue("@issuer", issuer);
                cmd.Parameters.AddWithValue("@reason", message);
                cmd.Parameters.AddWithValue("@time", Main.FormatDate(DateTime.Now + timeSpan));

                BanKick(ent, issuer, message);

                var spanstr = $"{timeSpan.Days}d{timeSpan.Hours}h{timeSpan.Minutes}m";
                Common.SayAll($"%p{ent.GetFormattedName()} %nhas been ^1tempbanned %nby %p{issuer}%n for {spanstr}. Reason: %i{message}");

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

            DelayedKick(ent, $"You are banned by %p{issuer}%n for {spanstr}. Reason: %i{message}".ColorFormat());
        }

        public static void Unwarn(Entity ent, string issuer, string reason = "You have been unwarned")
        {
            IEnumerator routine()
            {
                var cmd = new SQLiteCommand("SELECT amount FROM warnings WHERE hwid = @hwid;", Main.Connection);

                cmd.Parameters.AddWithValue("@hwid", ent.HWID);

                yield return Async.Detach();

                int amount = 0;
                lock (Main.Connection)
                {
                    var value = cmd.ExecuteScalar();

                    if (value != null)
                        amount = (int)value;
                }

                yield return Async.Attach();

                amount--;

                if (amount <= 0)
                    amount = 0;

                Common.SayAll($"%p{ent.GetFormattedName()} %nhas been ^3unwarned({amount}/3) %nby %p{issuer}%n. Reason: %i{reason}");

                cmd.Parameters.Clear();
                if (amount == 0)
                {
                    cmd.CommandText = "DELETE FROM warnings WHERE hwid = @hwid;";

                    cmd.Parameters.AddWithValue("@hwid", ent.HWID);
                }
                else
                {
                    cmd.CommandText = "INSERT OR REPLACE INTO warnings (hwid, amount) VALUES (@hwid, @amount);";

                    cmd.Parameters.AddWithValue("@hwid", ent.HWID);
                    cmd.Parameters.AddWithValue("@amount", amount);
                }

                yield return Async.Detach();

                lock (Main.Connection)
                {
                    cmd.ExecuteNonQuery();
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

                int amount = 0;
                lock (Main.Connection)
                {
                    var value = cmd.ExecuteScalar();

                    if (value != null)
                        amount = (int)value;
                }

                yield return Async.Attach();

                amount++;

                Common.SayAll($"%p{ent.GetFormattedName()} %nhas been ^2warned({amount}/3) %nby %p{issuer}%n. Reason: %i{reason}");

                if (amount >= MaxWarnings)
                {
                    Common.Admin.TempBan(ent, issuer, reason);
                    amount = 0;
                }

                cmd.CommandText = "INSERT OR REPLACE INTO warnings (hwid, amount) VALUES (@hwid, @amount);";

                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@hwid", ent.HWID);
                cmd.Parameters.AddWithValue("@amount", amount);

                yield return Async.Detach();

                lock (Main.Connection)
                {
                    cmd.ExecuteNonQuery();
                }
            }

            Async.Start(routine());
        }
    }
}

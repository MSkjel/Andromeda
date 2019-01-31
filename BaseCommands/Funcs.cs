using Andromeda;
using InfinityScript;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace BaseAdmin
{
    public static class Funcs
    {
        private static void DelayedKick(Entity ent, string message)
        {
            var cmd = $"dropclient {ent.EntRef} \"{message}\"";
            BaseScript.AfterDelay(100, delegate
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

                yield return Async.Detach();
                lock (Main.Connection)
                {
                    cmd.ExecuteNonQuery();
                }

                yield return Async.Attach();
                BanKick(ent, issuer, message);

                Common.SayAll($"%p{ent.GetFormattedName()} %nhas been ^1banned %nby %p{issuer}%n. Reason: %i{message}");
            }

            Async.Start(routine());
        }

        internal static void BanKick(Entity ent, string issuer, string message)
            => DelayedKick(ent, $"You are permanently banned by %p{issuer}%n. Reason: %i{message}".ColorFormat());

        public static void Kick(Entity ent, string issuer, string message = "You have been kicked")
        {
            DelayedKick(ent, $"Kicked by %p{issuer}%n. Reason: %i{message}\"".ColorFormat());

            Common.SayAll($"%p{ent.GetFormattedName()} %nhas been ^1kicked %n by %p{issuer}%n. Reason: %i{message}");
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

                yield return Async.Detach();
                lock (Main.Connection)
                {
                    cmd.ExecuteNonQuery();
                }

                yield return Async.Attach();
                BanKick(ent, issuer, message);

                var spanstr = $"{timeSpan.Days}d{timeSpan.Hours}h{timeSpan.Minutes}m";
                Common.SayAll($"%p{ent.GetFormattedName()} %nhas been ^1tempbanned %nby %p{issuer}%n for {spanstr}. Reason: %i{message}");
            }

            Async.Start(routine());
        }

        internal static void TmpBanKick(Entity ent, string issuer, TimeSpan timeSpan, string message)
        {
            var spanstr = $"{timeSpan.Days}d{timeSpan.Hours}h{timeSpan.Minutes}m";

            DelayedKick(ent, $"You are banned by %p{issuer}%n for {spanstr}. Reason: %i{message}".ColorFormat());
        }

        public static void Unwarn(Entity ent, string issuer, string reason = "You have been unwarned")
        {
            throw new NotImplementedException();
        }

        public static void Warn(Entity ent, string issuer, string reason = "You have been warned")
        {
            throw new NotImplementedException();
        }
    }
}

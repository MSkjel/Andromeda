using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;
using System.Collections;
using Andromeda;
using System.Data.SQLite;

namespace BaseAdmin
{
    public static class Funcs
    {
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
                lock(Main.Connection)
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
        {
            Utilities.ExecuteCommand($"kickclient {ent.EntRef} \"You have been permanently banned by %p{issuer}%n.\nReason: %i{message}".ColorFormat());
        }

        public static void Kick(Entity ent, string issuer, string message = "You have been kicked")
        {
            Utilities.ExecuteCommand($"dropclient {ent.EntRef} \"You have been kicked by %p{issuer}%n.\nReason: %i{message}".ColorFormat());

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
            Utilities.ExecuteCommand($"dropclient {ent.EntRef} \"You have been temporarily banned by %p{issuer}%n for {spanstr}.\nReason: %i{message}".ColorFormat());
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

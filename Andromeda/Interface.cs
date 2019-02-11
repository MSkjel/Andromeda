using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using InfinityScript;

namespace Andromeda
{
    public static partial class Common
    {
        public static void SayAll(IEnumerable<string> messages)
        {
            SayAllPlayers(messages);
            SayAllChattables(messages);
        }

        public static void SayAllPlayers(IEnumerable<string> messages)
            => Utils.SayAllPlayers(messages.Select(msg => msg.ColorFormat()));

        public static void SayAllChattables(IEnumerable<string> messages)
        {
            foreach (var chattable in chattables)
                chattable.RawSay(messages);
        }

        public static void SayAll(string message)
            => SayAll(message.Yield());

        public static void Tell(this Entity player, IEnumerable<string> messages)
            => Utils.SayToPlayer(player, messages.Select(msg => msg.ColorFormat()));

        public static void Tell(this Entity player, string message)
            => player.Tell(message.Yield());

        public static void Tell(this IClient chattable, IEnumerable<string> messages)
            => chattable.RawTell(messages.Select(msg => msg.ColorFormat()));

        public static void Tell(this IClient chattable, string message)
            => chattable.RawTell(message.Yield());

        public static string GetFormattedName(this Entity player)
            => Perms.GetFormattedName(player);

        private static Dictionary<string, string> colorScheme = Utils.ColorScheme.Export();

        public static string ColorFormat(this string message)
        {
            var sb = new StringBuilder("%n" + message);

            foreach(var kvp in colorScheme)
                sb.Replace(kvp.Key, kvp.Value);

            return Regex.Replace(sb.ToString(), @"(?:\^[\d;:]( *))+(\^[\d;:])", "$1$2");
        }

        public static int ColorlessLength(this string message)
            => Regex.Replace(message, @"\^[0-9;:]", "").Length;

        public static bool RequestPermission(this Entity player, string permission, out string message)
            => Perms.RequestPermission(player, permission, out message);

        internal static readonly Dictionary<string, object> Exports = new Dictionary<string, object>();

        public static void Export(string name, object obj)
            => Exports[name] = obj;

        public static void ExportAs<T>(string name, object obj)
            => Export(name, (T)obj);

        public static object Import(string name)
        {
            if (Exports.TryGetValue(name, out var ret))
                return ret;

            return null;
        }

        public static T GetImportOr<T>(string name, T defaultValue = default)
        {
            if (GetImport<T>(name, out var ret))
                return ret;

            return defaultValue;
        }

        public static bool GetImport<T>(string name, out T val)
        {
            if (Import(name) is T import)
            {
                val = import;
                return true;
            }

            val = default(T);
            return false;
        }

        #region Fields
        public static T GetFieldOrVal<T>(this Entity ent, string field, T def = default(T))
        {
            try
            {
                return ent.GetField<T>(field);
            }
            catch (Exception)
            {
                return def;
            }
        }

        public static void SetFieldT(this Entity ent, string field, object args)
        {
            if (args is int)
                ent.SetField(field, new Parameter((int)args));
            else if (args is bool)
                ent.SetField(field, new Parameter((bool)args ? 1 : 0));
            else if (args is float)
                ent.SetField(field, new Parameter((float)args));
            else if (args is string)
                ent.SetField(field, new Parameter((string)args));
            else if (args is Vector3)
                ent.SetField(field, new Parameter((Vector3)args));
            else if (args is Entity)
                ent.SetField(field, new Parameter((Entity)args));
            else
                ent.SetField(field, new Parameter(args));
        }

        public static bool IsFieldTrue(this Entity ent, string field)
            => ent.HasField(field) && ent.GetField<int>(field) != 0;
        public static bool IsFieldEqual(this Entity ent, string field, int limit)
            => ent.HasField(field) && ent.GetField<int>(field) == limit;
        public static bool IsFieldHigherOrEqual(this Entity ent, string field, int limit)
            => ent.HasField(field) && ent.GetField<int>(field) >= limit;
        public static bool IsFieldHigher(this Entity ent, string field, int limit)
            => ent.HasField(field) && ent.GetField<int>(field) > limit;
        public static bool IsFieldLowerOrEqual(this Entity ent, string field, int limit)
            => ent.HasField(field) && ent.GetField<int>(field) <= limit;
        public static bool IsFieldLower(this Entity ent, string field, int limit)
            => ent.HasField(field) && ent.GetField<int>(field) < limit;

        public static int IncrementField(this Entity ent, string field, int amount)
        {
            ent.SetField(field, ent.GetFieldOrVal<int>(field) + amount);

            return ent.GetField<int>(field);
        }

        public static int DecrementField(this Entity ent, string field, int amount)
        {
            int val = ent.GetFieldOrVal<int>(field) - amount;
            ent.SetField(field, val < 0 ? 0 : val);

            return ent.GetField<int>(field);
        }
        #endregion
    }
}

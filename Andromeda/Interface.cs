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
            => Utils.SayAll(messages.Select(msg => msg.ColorFormat()));

        public static void SayAll(string message)
            => SayAll(message.Yield());

        public static void Tell(this Entity player, IEnumerable<string> messages)
            => Utils.SayTo(player, messages.Select(msg => msg.ColorFormat()));

        public static void Tell(this Entity player, string message)
            => player.Tell(message.Yield());

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

        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }
    }
}

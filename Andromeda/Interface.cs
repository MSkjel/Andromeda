using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;

namespace Andromeda
{
    public static partial class Common
    {
        public static void SayAll(IEnumerable<Msg> messages)
            => Utils.SayAll(messages);

        public static void SayAll(Msg message)
            => SayAll(message.Yield());

        public static void Tell(this Entity player, IEnumerable<Msg> messages)
            => Utils.SayTo(player, messages);

        public static void Tell(this Entity player, Msg message)
            => player.Tell(message.Yield());

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

        public static T GetImportOr<T>(string name, T defaultValue)
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

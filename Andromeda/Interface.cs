using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;

namespace CommonFunctionality
{
    public static partial class Common
    {
        public static void SayAll(IEnumerable<Msg> messages)
            => Utils.SayAll(messages);
        public static void SayAll(Msg message)
            => SayAll(new[] { message });

        public static void Tell(this Entity player, IEnumerable<Msg> messages)
            => Utils.SayTo(player, messages);

        public static void Tell(this Entity player, Msg message)
            => player.Tell(new[] { message });

        public static bool HasPermission(this Entity player, string permission)
            => Perms.HasPermission(player, permission);

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

    }
}

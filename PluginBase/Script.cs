using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using InfinityScript.PBase;

namespace InfinityScript
{
    public static partial class Script
    {
        internal static PluginBase Instance;

        internal static Func<Exception, bool> ErrorHandler(string eventName)
        {
            return delegate (Exception ex)
            {
                PrintException(ex, $"Exception during processing of event {eventName}");
                return false;
            };
        }

        internal static void PrintException(Exception ex, string context)
        {
            Error(new[]
            {
                context,
                $"Exception: {ex.Message}",
                $"Stacktrace: {ex.StackTrace}"
            });
        }

        internal static void Error(IEnumerable<string> messages)
        {
            Log.Error("=========== PluginBase ============");
            foreach (var msg in messages)
                Log.Error(msg);
            Log.Error("===================================");
        }

        internal static void Init()
        {
            Log.Info("Initialized PluginBase.");

            Log.Info("Loading plugins...");
            LoadPlugins();
            Log.Info("Done loading plugins.");
                 
        }

        internal static void Cleanup()
        {
            Log.Info("Cleaning up plugins...");
            foreach (var plugin in LoadedPlugins)
                plugin.RunCleanups();

            Log.Info("Done cleaning plugins.");
        }
    }
}

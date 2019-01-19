using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PBase;
using CommonFunctionality.Interfaces;
using InfinityScript;
using CommonFunctionality.Cmd;

namespace CommonFunctionality
{
    [Plugin]
    public static partial class Common
    {
        public static string Version
            => "Andromeda v1.0.0";
        public static IPerms Perms { get; private set; }
        public static IUtils Utils { get; private set; }
        public static IAdmin Admin { get; private set; }

        internal static readonly List<IFunctionality> functionalities = new List<IFunctionality>();
        
        internal static readonly Dictionary<string, object> Exports = new Dictionary<string, object>();

        public static void PrintException(string message, string source)
            => Exception(new[]
            {
                $"Source: {source}",
                message,
            });

        public static void PrintException(Exception exception, string source)
            => Exception(new[] {
                $"Source: {source}",
                exception.Message,
                exception.StackTrace
            });

        public static void Exception(params string[] lines)
        {
            Log.Error("----------- Exception ------------");
            foreach (var line in lines)
                Log.Error(line);
            Log.Error("----------------------------------");
        }

        public static void Warning(params string[] lines)
        {
            Log.Info("------------- Warning ------------");
            foreach (var line in lines)
                Log.Info(line);
            Log.Info("----------------------------------");
        }

        public static string Format(this string str, Dictionary<string, string> format)
        {
            var builder = new StringBuilder(str);

            foreach (var val in format)
                builder.Replace(val.Key, val.Value);

            return builder.ToString();
        }

        public static void Register(IFunctionality functionality)
        {
            switch(functionality)
            {
                case IPerms perms:
                    if (Perms == null)
                    {
                        Perms = perms;
                        Exports[nameof(Perms)] = Perms;
                    }
                    else
                        Warning("Perms already assigned", "Ignoring new register");
                    return;
                case IUtils utils:
                    if (Utils == null)
                    {
                        Utils = utils;
                        Exports[nameof(Utils)] = Utils;
                    }
                    else
                        Warning("Utils already assigned", "Ignoring new register");
                    return;
                case IAdmin admin:
                    if (Admin == null)
                    {
                        Admin = admin;
                        Exports[nameof(Admin)] = Admin;
                    }
                    else
                        Warning("Admin already assigned", "Ignoring new register");
                    return;
            }

            functionalities.Add(functionality);
        }

        [EntryPoint]
        private static void Init()
        {
            Log.Info("Initializing CommonFunctionality");
            if(Perms == null)
            {
                Warning(new[]
                {
                    "IPerms has not been registered",
                    "Mock IPerms will be used",
                });

                Register(new Mock.Perms());
            }

            if (Utils == null)
            {
                Warning(new[]
                {
                    "IUtils has not been registered",
                    "Mock IUtils will be used",
                });

                Register(new Mock.Utils());
            }

            if (Admin == null)
            {
                Warning(new[]
{
                    "IAdmin has not been registered",
                    "Mock IAdmin will be used",
                });

                Register(new Mock.Admin());
            }
        }

        static Common()
        {
            // VERSION
            Command.TryRegister(ArgParse.CreateCommand(
                name: "version",
                aliases: new[] { "v" },
                argTypes: new IArgParse[0],
                action: delegate (Entity sender, object[] args)
                {
                    sender.Tell(new Msg[] {
                        "Versions:",
                        Msg.Extra(Version),
                    }.Concat(
                        functionalities.Select(func => Msg.Extra(func.Version))
                    ));
                },
                usage: "!version",
                description: "Shows versions of functionalities"));
        }
    }
}

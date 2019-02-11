using Andromeda.Events;
using Andromeda.Interfaces;
using Andromeda.Parse;
using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Andromeda
{
    [Plugin]
    public static partial class Common
    {
        internal static readonly string Version = "Andromeda v0.9.1";

        internal static readonly string[] Credits = new[]
        {
            "Lambder & Markus - dem bois",
            "Slvr99 - help-ish with AbortScript"
        };

        public static readonly string ConfigsFolder;

        private static IPerms perms;
        private static readonly IPerms mockPerms = new Mock.Perms();
        public static IPerms Perms
            => perms ?? mockPerms;

        private static IUtils utils;
        private static readonly IUtils mockUtils = new Mock.Utils();
        public static IUtils Utils
            => utils ?? mockUtils;

        private static IAdmin admin;
        private static readonly IAdmin mockAdmin = new Mock.Admin();
        public static IAdmin Admin
            => admin ?? mockAdmin;

        internal static readonly List<IFunctionality> functionalities = new List<IFunctionality>();
        internal static readonly List<IClient> clients = new List<IClient>();

        public static readonly IClient Console = new ConsoleWrapper();

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

        public static void Register(IFunctionality functionality)
        {
            if (functionality is IPerms perms)
            {
                if (Common.perms == null)
                {
                    Common.perms = perms;
                    Exports[nameof(Perms)] = Perms;
                }
                else
                    Warning("Perms already assigned", $"Ignoring new register: {perms.Version}");
            }

            if (functionality is IUtils utils)
            {
                if (Common.utils == null)
                {
                    Common.utils = utils;
                    Exports[nameof(Utils)] = Utils;

                    colorScheme = utils.ColorScheme.Export();
                }
                else
                    Warning("Utils already assigned", $"Ignoring new register: {utils.Version}");
            }

            if (functionality is IAdmin admin)
            {
                if (Common.admin == null)
                {
                    Common.admin = admin;
                    Exports[nameof(Admin)] = Admin;
                }
                else
                    Warning("Admin already assigned", $"Ignoring new register: {admin.Version}");
            }

            functionalities.Add(functionality);
        }

        public static void AddClient(IClient client)
            => clients.Add(client);

        public static void RemoveClient(IClient client)
            => clients.Remove(client);

        [EntryPoint]
        private static void Init()
        {
            Log.Info("Initializing Andromeda");

            AddClient(Console);

            if (perms == null)
            {
                Warning(new[]
                {
                    "IPerms has not been registered",
                    "Mock IPerms will be used",
                });
            }

            if (utils == null)
            {
                Warning(new[]
                {
                    "IUtils has not been registered",
                    "Mock IUtils will be used",
                });
            }

            if (admin == null)
            {
                Warning(new[]
{
                    "IAdmin has not been registered",
                    "Mock IAdmin will be used",
                });
            }
        }

        static Common()
        {
            // VERSION
            Command.TryRegister(SmartParse.CreateCommand(
                name: "version",
                aliases: new[] { "v" },
                argTypes: null,
                action: delegate (IClient sender, object[] args)
                {
                    sender.Tell(new[]
                    {
                        "%iVersions:",
                        Version,
                    }.Concat(
                        functionalities.Select(func => func.Version)
                    ));
                },
                usage: "!version",
                description: "Shows versions of functionalities"));

            // CREDITS
            Command.TryRegister(SmartParse.CreateCommand(
                name: "credits",
                argTypes: null,
                action: delegate (IClient sender, object[] args)
                {
                    var msg = "%iVersions:"
                        .Append($"%h1{Version}")
                        .Concat(Credits);

                    foreach (var func in functionalities)
                    {
                        msg = msg
                            .Append($"%h1{func.Version}")
                            .Concat(func.Credits);
                    }

                    sender.Tell(msg);
                },
                usage: "!credits"));
        }
    }
}

using Andromeda.Interfaces;
using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Andromeda.Parse;

namespace Andromeda
{
    [Plugin]
    public class Command
    {
        #region Static
        internal static readonly SortedList<string, Command> RegisteredCommands = new SortedList<string, Command>();
        private static readonly SortedList<string, Command> aliasLookup = new SortedList<string, Command>();

        public static Command Lookup(string str)
        {
            if (aliasLookup.TryGetValue(str, out var cmd))
                return cmd;

            return null;
        }

        public static bool CanDo(IClient client, Command cmd, out string response)
        {
            if (cmd.permission == null)
            {
                response = "Command has no permission";
                return true;
            }

            return client.RequestPermission(cmd.permission, out response);
        }

        public static bool TryRegister(Command cmd)
        {
            if (cmd?.Name == null)
                return false;

            string name = cmd.Name.ToLowerInvariant();

            if (RegisteredCommands.ContainsKey(name))
                return false;

            RegisteredCommands[name] = cmd;
            aliasLookup[name] = cmd;

            if (cmd.Aliases != null)
                foreach (var alias in cmd.Aliases)
                {
                    var lowerAlias = alias.ToLowerInvariant();

                    if (!aliasLookup.ContainsKey(lowerAlias))
                        aliasLookup[lowerAlias] = cmd;
                }

            return true;
        }

        public static bool TryRegister(string name, Action<IClient, string> action, string usage, string[] aliases = null, string permission = null, string description = null)
            => TryRegister(new Command(name, action, usage, aliases, permission, description));

        internal static void Process(IClient sender, string message)
        {
            if (SmartParse.CommandName.Parse(ref message, out var parse, sender) is string error)
                sender.Tell($"%e{error}");
            else
            {
                var cmdName = ((string)parse).ToLowerInvariant();

                Command cmd = Lookup(cmdName);

                if (cmd == null)
                    sender.Tell($"%eNo such command: {cmdName}");
                else if (!CanDo(sender, cmd, out var err))
                    sender.Tell($"%e{err}");
                else
                    cmd.TryRun(sender, message);
            }
        }

        static Command()
        {
            // Command processing
            Script.PlayerSay.Add((sender, args) =>
            {
                if(args.ChatType == BaseScript.ChatType.All
                    && args.Message.StartsWith("!"))
                {
                    var message = args.Message;
                    var client = new EntityWrapper(args.Player);

                    Process(client, message);

                    args.Eat();
                }
            }, int.MinValue);

            #region Commands
            // HELP
            TryRegister(SmartParse.CreateCommand(
                name: "help",
                argTypes: new[] { SmartParse.OptionalString },
                action: delegate (IClient sender, object[] args)
                {
                    if(args[0] is string filter)
                    {
                        filter = filter.ToLowerInvariant();

                        IEnumerable<Command> cmds;

                        if (RegisteredCommands.TryGetValue(filter, out var singleCmd))
                        {
                            cmds = new[] { singleCmd };
                        }
                        else
                        {
                            cmds = RegisteredCommands.Values.Where(x =>
                            {
                                if (x.Name.Contains(filter))
                                    return true;

                                if (x.Aliases.Any(alias => alias.Contains(filter)))
                                    return true;

                                return false;
                            });
                        }

                        if(!cmds.Any())
                        {
                            sender.Tell("%eNo commands were found.");
                            return;
                        }

                        if(cmds.Count() == 1)
                        {
                            var cmd = cmds.First();

                            sender.Tell(new[]
                            {
                                $"Command: %i!{cmd.Name}",
                                $"Usage: %i{cmd.Usage}",
                                cmd.Description ?? "No description given",
                            });

                            return;
                        }

                        sender.Tell($"%iCommands matching {filter}:".Yield().Concat(
                            cmds.Select(c => $"!{c.Name}")
                                .Condense()));

                        return;
                    }

                    sender.Tell("%iAvailable commands:".Yield().Concat(
                        RegisteredCommands.Values
                            .Where(cmd => CanDo(sender, cmd, out _))
                            .Select(cmd => $"!{cmd.Name}")
                            .Condense()));
                },
                usage: "!help [filter]",
                description: "Filters commands or shows description if a single command was found"));

            // PM
            TryRegister(SmartParse.CreateCommand(
                name: "pm",
                argTypes: new[] { SmartParse.Player, SmartParse.GreedyString },
                action: delegate (IClient sender, object[] args)
                {
                    var target = args[0] as Entity;
                    var message = args[1] as string;

                    target.Tell($"%p{sender.Name} %i-> You: %h1{message}");
                    sender.Tell($"%iYou -> %p{target.Name}%i: %h1{message}");

                    if (sender.IsEntity)
                    {
                        sender.Entity.SetField("pm.target", target.EntRef);
                        target.SetField("pm.target", sender.Entity.EntRef);
                    }
                },
                usage: "!pm <player> <message>",
                description: "Sends a private message to a player"));

            // REPLY
            TryRegister(SmartParse.CreateCommand(
                name: "reply",
                aliases: new[] { "r" },
                argTypes: new[] { SmartParse.GreedyString },
                action: delegate (Entity sender, object[] args)
                {
                    var message = args[0];

                    if(!sender.HasField("pm.target"))
                    {
                        sender.Tell("%eYou have nobody to reply to.");
                        return;
                    }

                    var target = Entity.GetEntity(sender.GetField<int>("pm.target"));

                    if(!BaseScript.Players.Contains(target))
                    {
                        sender.Tell("%eTarget is no longer connected.");
                        sender.ClearField("pm.target");
                        return;
                    }

                    target.Tell($"%p{sender.Name} %i-> You: %h1{message}");
                    sender.Tell($"%iYou -> %p{target.Name}%i: %h1{message}");
                },
                usage: "!reply <message>",
                description: "Reply to a received pm"));

            // USAGE
            TryRegister(SmartParse.CreateCommand(
                name: "usage",
                argTypes: new[] { SmartParse.String },
                action: delegate (IClient sender, object[] args)
                {
                    var alias = args[0] as string;

                    if (aliasLookup.TryGetValue(alias, out var cmd))
                        sender.Tell($"Usage: %i{cmd.Usage}");
                    else
                        sender.Tell($"%eNo such command: !{alias}");
                },
                usage: "!usage <cmd/alias>",
                description: "Shows usage of a command or an alias"));

            // STATUS
            TryRegister(SmartParse.CreateCommand(
                name: "status",
                argTypes: null,
                action: delegate (IClient sender, object[] args)
                {
                    var response = "%aOnline players:".Yield()
                    .Concat(BaseScript.Players
                        .Select(player => $"%h1{player.EntRef} %n- %p{player.Name} %n- %h2{player.IsLogged()}"));

                    sender.Tell(response);
                },
                usage: "!status",
                description: "Shows online players and their slots"));
            #endregion
        }
        #endregion

        public readonly string Name;

        public readonly string[] Aliases;

        public readonly string Usage;

        protected readonly string permission;

        public readonly string Description;

        protected readonly Action<IClient, string> action;

        public Command(string name, Action<IClient, string> action, string usage, string[] aliases = null, string permission = null, string description = null)
        {
            Name = name;
            this.action = action;
            Aliases = aliases ?? new string[0];
            Usage = usage;
            this.permission = permission;
            Description = description;
        }

        public void TryRun(IClient sender, string message)
        {
            try
            {
                action(sender, message);
            }
            catch(Exception ex)
            {
                var response = new[]
                {
                    "%eError running command",
                    "Check console for more details."
                };

                sender.Tell(response);
                Common.PrintException(ex, $"Command {Name}");
            }
        }
    }
}

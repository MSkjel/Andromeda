using CommonFunctionality.Interfaces;
using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PBase;
using CommonFunctionality.Cmd;

namespace CommonFunctionality
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

        public static bool CanDo(Entity player, Command cmd)
        {
            if (cmd.permission == null)
                return true;

            return player.HasPermission(cmd.permission);
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

        public static bool TryRegister(string name, Action<Entity, string> action, string usage, string[] aliases = null, string permission = null, string description = null)
            => TryRegister(new Command(name, action, usage, aliases, permission, description));

        static Command()
        {
            // Command processing
            Script.PlayerSay.Add((sender, args) =>
            {
                if(args.Message.StartsWith("!"))
                {
                    var message = args.Message;

                    if(ArgParse.CommandName.Parse(ref message, out var parse) is string error)
                        args.Player.Tell(Msg.Error(error));
                    else
                    {
                        var cmdName = ((string)parse).ToLowerInvariant();

                        Command cmd = Lookup(cmdName);

                        if (cmd == null)
                            args.Player.Tell(Msg.Error($"No such command: {cmdName}"));
                        else if (!CanDo(args.Player, cmd))
                            args.Player.Tell(Msg.Error($"You may not use that command"));
                        else
                            cmd.TryRun(args.Player, message);
                    }

                    args.Eat();
                }
            }, int.MinValue);

            #region Commands
            // HELP
            TryRegister(ArgParse.CreateCommand(
                name: "help",
                argTypes: new[] { ArgParse.OptionalString },
                action: delegate (Entity sender, object[] args)
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
                            sender.Tell(Msg.Error("No commands were found."));
                            return;
                        }

                        if(cmds.Count() == 1)
                        {
                            var cmd = cmds.First();

                            sender.Tell(new Msg[]
                            {
                                $"Command: !{cmd.Name}",
                                $"Usage: {cmd.Usage}",
                                cmd.Description ?? "No description given",
                            });

                            return;
                        }

                        sender.Tell(new Msg[]
                        {
                            $"Commands matching {filter}:",
                            Msg.Extra(string.Join(", ", cmds.Select(c => $"!{c.Name}"))),
                        });

                        return;
                    }

                    sender.Tell(new Msg[]
                    {
                        "Available commands:",
                        Msg.Extra(string.Join(", ", RegisteredCommands.Values.Where(cmd => CanDo(sender, cmd)).Select(cmd => $"!{cmd.Name}")))
                    });
                },
                usage: "!help [filter]",
                description: "Filters commands or shows description if a single command was found"));

            // PM
            TryRegister(ArgParse.CreateCommand(
                name: "pm",
                argTypes: new[] { ArgParse.Player, ArgParse.GreedyString },
                action: delegate (Entity sender, object[] args)
                {
                    var target = args[0] as Entity;
                    var message = args[1] as string;

                    target.Tell($"{sender.Name}: {message}");
                },
                usage: "!pm <player> <message>",
                description: "Sends a private message to a player"));

            // USAGE
            TryRegister(ArgParse.CreateCommand(
                name: "usage",
                argTypes: new[] { ArgParse.String },
                action: delegate (Entity sender, object[] args)
                {
                    var alias = args[0] as string;

                    if (aliasLookup.TryGetValue(alias, out var cmd))
                        sender.Tell(new Msg[]
                        {
                            "Usage:",
                            Msg.Extra(cmd.Usage),
                        });
                    else
                        sender.Tell(Msg.Error($"No such command: !{alias}"));
                },
                usage: "!usage <cmd/alias>",
                description: "Shows usage of a command or an alias"));
            #endregion
        }
        #endregion

        public readonly string Name;

        public readonly string[] Aliases;

        public readonly string Usage;

        protected readonly string permission;

        public readonly string Description;

        protected readonly Action<Entity, string> action;

        public Command(string name, Action<Entity, string> action, string usage, string[] aliases = null, string permission = null, string description = null)
        {
            Name = name;
            this.action = action;
            Aliases = aliases ?? new string[0];
            Usage = usage;
            this.permission = permission;
            Description = description;
        }

        public void TryRun(Entity sender, string message)
        {
            try
            {
                action(sender, message);
            }
            catch(Exception ex)
            {
                var response = new Msg[]
                {
                    Msg.Error("Error running command"),
                    "Check console for more details."
                };

                sender.Tell(response);
                Common.PrintException(ex, $"Command {Name}");
            }
        }
    }
}

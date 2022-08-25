using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Andromeda.Parse
{
    public static class SmartParse
    {
        public static readonly IArgParse OptionalString = new OptionalStringParse();
        public static readonly IArgParse String = new StringParse();
        public static readonly IArgParse OptionalGreedyString = new OptionalGreedyStringParse();
        public static readonly IArgParse GreedyString = new GreedyStringParse();
        public static readonly IArgParse Player = new PlayerParse();
        public static readonly IArgParse OptionalPlayer = new OptionalPlayerParse();
        public static readonly IArgParse OptionalInteger = new OptionalIntegerParse();
        public static readonly IArgParse Integer = new IntegerParse();
        public static readonly IArgParse OptionalBoolean = new OptionalBooleanParse();
        public static readonly IArgParse Boolean = new BooleanParse();

        public static readonly IArgParse UnimmunePlayer = new ConstraintParse(Player, delegate (object obj, IClient client)
        {
            if(client.IsEntity)
                if (Common.Perms.IsImmuneTo(obj as Entity, client.Entity))
                    return "Player is immune";

            return null;
        });
        public static readonly IArgParse LoggedInPlayer = new ConstraintParse(Player, delegate (object obj, IClient client)
        {
            if ((obj as Entity).IsLogged())
                return null;

            return "Player is not logged";
        });
        public static IArgParse RangedInteger(int min, int max)
            => new ConstraintParse(Integer, delegate (object obj, IClient client)
            {
                var x = (int)obj;

                if (x < min || x > max)
                    return $"Integer is not in range({min}-{max})";

                return null;
            });
        public static IArgParse OptionalRangedIntegerWithDefault(int min, int max, int def)
            => new ConstraintParse(OptionalInteger, delegate (object obj, IClient client)
            {
                var x = obj as int? ?? def;

                if (x < min || x > max)
                    return $"Integer is not in range({min}-{max})";

                return null;
            });

        internal static readonly IArgParse CommandName = new CommandNameParse();

        public static Command CreateCommand(string name, IArgParse[] argTypes, Action<Entity, object[]> action, string usage,
            string[] aliases = null, string permission = null, string description = null)
            => CreateCommand(name, argTypes,
                delegate (IClient sender, object[] args)
                {
                    //if(!sender.IsEntity)
                    //{
                    //    sender.Tell("%eYou must be a player to use this command.");
                    //    return;
                    //}

                    action(sender.Entity, args);
                }, usage, aliases, permission, description);

        public static Command CreateCommand(string name, IArgParse[] argTypes, Action<IClient, object[]> action, string usage,
            string[] aliases = null, string permission = null, string description = null)
                        => new Command(name,
                delegate (IClient sender, string message)
                {
                    if (argTypes != null)
                    {
                        object[] arguments = new object[argTypes.Length];

                        for (int i = 0; i < argTypes.Length; i++)
                            if (argTypes[i].Parse(ref message, out arguments[i], sender) is string error)
                            {
                                var response = new[]
                                {
                                    $"%e({i + 1}) {error}",
                                    $"Usage: %i{usage}",
                                };

                                sender.Tell(response);
                                return;
                            }

                        if (string.IsNullOrEmpty(message))
                            action(sender, arguments);
                        else
                            sender.Tell(new[]
                            {
                                "%eToo many arguments given",
                                $"Usage: %i{usage}"
                            });
                    }
                    else
                        action(sender, null);

                }, usage, aliases, permission, description);
    }

    public interface IArgParse
    {
        string Parse(ref string str, out object parsed, IClient sender);
    }

    public class OptionalStringParse : IArgParse
    {
        public virtual string Parse(ref string str, out object parsed, IClient sender)
        {
            var match = Regex.Match(str, @"(\S+)(?:\s+(.*))?");

            if (match.Success)
            {
                parsed = match.Groups[1].Value;
                str = match.Groups[2].Value;

                if (string.IsNullOrEmpty(parsed as string))
                    parsed = null;

                return null;
            }

            str = string.Empty;
            parsed = null;
            return null;
        }
    }

    public class StringParse : OptionalStringParse
    {
        public override string Parse(ref string str, out object parsed, IClient sender)
        {
            base.Parse(ref str, out parsed, sender);

            if (string.IsNullOrEmpty(parsed as string))
                return "Expected string";

            return null;
        }
    }

    public class OptionalGreedyStringParse : IArgParse
    {
        public virtual string Parse(ref string str, out object parsed, IClient sender)
        {
            if (str == string.Empty)
            {
                parsed = null;
                return null;
            }

            parsed = str;
            str = string.Empty;

            return null;
        }
    }

    public class GreedyStringParse : OptionalGreedyStringParse
    {
        public override string Parse(ref string str, out object parsed, IClient sender)
        {
            base.Parse(ref str, out parsed, sender);

            if (string.IsNullOrEmpty(parsed as string))
                return "Expected string";

            return null;
        }
    }

    public class OptionalPlayerParse : IArgParse
    {
        public virtual string Parse(ref string str, out object parsed, IClient sender)
        {
            SmartParse.OptionalString.Parse(ref str, out parsed, sender);

            if (parsed is string selector)
            {
                var match = Regex.Match(selector, @"^#(\d+)$");

                if (match.Success)
                {
                    string index = match.Groups[1].Value;

                    int.TryParse(index, out var slot);

                    if (slot > 18 || slot < 0)
                        return "Slot numbers are 0-17";

                    foreach (var player in BaseScript.Players)
                        if (player.EntRef == slot)
                        {
                            parsed = player;
                            return null;
                        }

                    return "Slot is not occupied";
                }

                selector = selector.ToLowerInvariant();

                var found = BaseScript.Players.Where(ent => ent.Name.ToLowerInvariant().Contains(selector));

                if (!found.Any())
                    return "No players found";

                if (found.Count() > 1)
                    return "More that one player found";

                parsed = found.First();
                return null;
            }

            return null;
        }
    }

    public class PlayerParse : OptionalPlayerParse
    {
        public override string Parse(ref string str, out object parsed, IClient sender)
        {
            if (base.Parse(ref str, out parsed, sender) is string error)
                return error;

            if (parsed == null)
                return "Expected player selector";

            return null;
        }
    }

    public delegate string Constraint(object obj, IClient sender);

    public class ConstraintParse : IArgParse
    {
        private readonly Constraint constraint;
        private readonly IArgParse parse;
        public ConstraintParse(IArgParse parse, Constraint constraint)
        {
            this.parse = parse;
            this.constraint = constraint;
        }

        public string Parse(ref string str, out object parsed, IClient sender)
        {
            if (parse.Parse(ref str, out parsed, sender) is string error)
                return error;

            if (constraint(parsed, sender) is string err)
                return err;

            return null;
        }
    }

    public class OptionalIntegerParse : IArgParse
    {
        public virtual string Parse(ref string str, out object parsed, IClient sender)
        {
            SmartParse.OptionalString.Parse(ref str, out parsed, sender);

            if (parsed == null)
                return null;

            if (int.TryParse(parsed as string, out var number))
            {
                parsed = number;
                return null;
            }

            return $"{parsed} is not an integer";
        }
    }

    public class IntegerParse : OptionalIntegerParse
    {
        public override string Parse(ref string str, out object parsed, IClient sender)
        {
            if (base.Parse(ref str, out parsed, sender) is string)
                return "Integer expected";

            if (parsed == null)
                return "Integer expected";

            return null;
        }
    }

    public class BooleanParse : OptionalBooleanParse
    {
        public override string Parse(ref string str, out object parsed, IClient sender)
        {
            if (base.Parse(ref str, out parsed, sender) is string)
                return "Expected boolean";

            if (parsed is bool)
                return null;

            return "Expected boolean";
        }
    }

    public class OptionalBooleanParse : IArgParse
    {
        public virtual string Parse(ref string str, out object parsed, IClient sender)
        {
            SmartParse.OptionalString.Parse(ref str, out parsed, sender);

            if (parsed is string option)
            {
                switch (option.ToLowerInvariant())
                {
                    case "true":
                    case "t":
                    case "1":
                    case "yes":
                    case "y":
                    case "enable":
                        parsed = true;
                        return null;
                    case "false":
                    case "f":
                    case "0":
                    case "no":
                    case "n":
                    case "disable":
                        parsed = false;
                        return null;
                }

                parsed = false;
                return null;
            }

            parsed = null;
            return null;
        }
    }

    internal class CommandNameParse : StringParse
    {
        public override string Parse(ref string str, out object parsed, IClient sender)
        {
            if (base.Parse(ref str, out parsed, sender) is string error)
                return error;

            if (parsed is string cmd)
            {
                var match = Regex.Match(cmd, @"^!(\w+)$");

                if (match.Success)
                {
                    parsed = match.Groups[1].Value;
                    return null;
                }
            }

            return "Invalid empty command";
        }
    }
}

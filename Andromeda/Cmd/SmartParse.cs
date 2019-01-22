﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;
using System.Text.RegularExpressions;

namespace Andromeda.Cmd
{
    public static class SmartParse
    {
        public static readonly IArgParse OptionalString = new OptionalStringParse();
        public static readonly IArgParse String = new StringParse();
        public static readonly IArgParse OptionalGreedyString = new OptionalGreedyStringParse();
        public static readonly IArgParse GreedyString = new GreedyStringParse();
        public static readonly IArgParse Player = new PlayerParse();

        internal static readonly IArgParse CommandName = new CommandNameParse();

        public static Command CreateCommand(string name, IArgParse[] argTypes, Action<Entity, object[]> action, string usage, string[] aliases = null, string permission = null, string description = null)
            => new Command(name,
                delegate (Entity sender, string message)
                {
                    object[] arguments = new object[argTypes.Length];

                    for(int i = 0; i < argTypes.Length; i++)
                        if(argTypes[i].Parse(ref message, out arguments[i], sender) is string error)
                        {
                            var response = new[]
                            {
                                $"Usage: %i{usage}",
                                $"%eError parsing argument {i}:",
                                error
                            };

                            sender.Tell(response);
                            return;
                        }

                    if (string.IsNullOrEmpty(message))
                        action(sender, arguments);
                    else
                        sender.Tell("%eToo many arguments given");

                }, usage, aliases, permission, description);
    }

    public interface IArgParse
    {
        string Parse(ref string str, out object parsed, Entity sender);
    }

    public class OptionalStringParse : IArgParse
    {
        public virtual string Parse(ref string str, out object parsed, Entity sender)
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
        public override string Parse(ref string str, out object parsed, Entity sender)
        {
            base.Parse(ref str, out parsed, sender);

            if (string.IsNullOrEmpty(parsed as string))
                return "Expected string";

            return null;
        }
    }

    public class OptionalGreedyStringParse : IArgParse
    {
        public virtual string Parse(ref string str, out object parsed, Entity sender)
        {
            if(str == string.Empty)
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
        public override string Parse(ref string str, out object parsed, Entity sender)
        {
            base.Parse(ref str, out parsed, sender);

            if (string.IsNullOrEmpty(parsed as string))
                return "Expected string";

            return null;
        }
    }

    public class PlayerParse : IArgParse
    {
        public virtual string Parse(ref string str, out object parsed, Entity sender)
        {
            if (SmartParse.String.Parse(ref str, out parsed, sender) is string error)
                return "Expected player selector";

            if(parsed is string selector)
            {
                var match = Regex.Match(selector, @"^#(\d+)$");

                if (match.Success)
                {
                    string index = match.Groups[1].Value;

                    int.TryParse(index, out var slot);

                    if (slot > 18 || slot < 0)
                        return "Slot numbers are 0-17";

                    foreach(var player in BaseScript.Players)
                        if(player.EntRef == slot)
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

            return "Parsed string is null?";
        }
    }

    public class UnimmunePlayerParse : PlayerParse
    {
        public override string Parse(ref string str, out object parsed, Entity sender)
        {
            if (base.Parse(ref str, out parsed, sender) is string error)
                return error;

            if(parsed is Entity player)
            {
                if (Common.Perms.IsImmuneTo(player, sender))
                {
                    parsed = null;

                    return "Player is immune.";
                }

                return null;
            }

            return "Parsed is not Entity?";
        }
    }

    internal class CommandNameParse : StringParse
    {
        public override string Parse(ref string str, out object parsed, Entity sender)
        {
            if (base.Parse(ref str, out parsed, sender) is string error)
                return error;

            if(parsed is string cmd)
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

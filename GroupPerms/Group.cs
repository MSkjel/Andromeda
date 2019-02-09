using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GroupPerms
{
    internal class Group
    {
        public string Name;
        public string NameFormat;
        public string[] Permissions;

        public bool CanDo(string permission, out string message)
        {
            var negated = $"-{permission}";
            foreach (var perm in Permissions)
            {
                if (perm == permission)
                {
                    message = "Group contains permission";
                    return true;
                }

                if (perm == "*ALL*")
                {
                    message = "Group contains all permissions";
                    return true;
                }

                if (perm == negated)
                {
                    message = "Group contains negated permission";
                    return false;
                }

                var match = Regex.Match(perm, @"inherit\.(\w+)");

                if (match.Success)
                {
                    var name = match.Groups[1].Value;
                    if (Main.GroupLookup.TryGetValue(name, out var group) && group.CanDo(permission, out message))
                        return true;
                }
            }

            message = "Group does not contain permission";
            return false;
        }

        public string FormatName(string name)
            => NameFormat.Replace("<name>", name);
    }
}

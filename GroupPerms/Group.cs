using Andromeda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using InfinityScript;

namespace GroupPerms
{
    internal class Group
    {
        public string Name;
        public string NameFormat;
        public string[] Inherit;
        public string[] Permissions;

        private IEnumerable<string> GetStarPermissions(string permission)
        {
            yield return "*";
            var split = permission.Split('.');

            if (split.Length == 0)
                yield break;

            var sb = new StringBuilder(split[0]);
            yield return $"{sb}.*";

            for (int i = 1; i < split.Length - 1; i++)
            {
                sb.Append(".");
                sb.Append(split[i]);
                yield return $"{sb}.*";
            }
        }

        private bool? RequestPermissionRaw(string[] positiveNodes, string[] negativeNodes, out string message)
        {
            foreach (var perm in Permissions)
            {
                foreach (var node in positiveNodes)
                    if (perm == node)
                    {
                        message = $"Found perm {perm}";
                        return true;
                    }

                foreach (var node in negativeNodes)
                    if (perm == node)
                    {
                        message = $"Found perm {perm}";
                        return false;
                    }
            }

            message = "Group does not contain permission";
            return null;
        }

        public bool RequestPermission(string permission, out string message)
        {
            var nodes = GetStarPermissions(permission).Append(permission).ToArray();

            var negatedNodes = nodes.Select(node => $"-{node}").ToArray();

            if (RequestPermissionRaw(nodes, negatedNodes, out message) is bool ret)
                return ret;

            if (Inherit != null)
                foreach (var groupName in Inherit)
                    if (Main.GroupLookup.TryGetValue(groupName, out var group) && group.RequestPermissionRaw(nodes, negatedNodes, out message) is bool found)
                        return found;

            message = "Permission not found";
            return false;
        }

        public string FormatName(string name)
            => NameFormat.Replace("<name>", name);
    }
}

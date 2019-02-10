using Andromeda.Parse;
using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Andromeda;

namespace GroupPerms.Parse
{
    class Group : IArgParse
    {
        public static Group Obj = new Group();

        public string Parse(ref string str, out object parsed, IClient sender)
        {
            if (SmartParse.String.Parse(ref str, out parsed, sender) is string)
                return "Expected group name";

            var grpName = parsed as string;

            if (Main.GroupLookup.TryGetValue(grpName, out var grp))
            {
                parsed = grp;
                return null;
            }

            return "No such group found";
        }
    }
}

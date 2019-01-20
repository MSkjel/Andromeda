using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;

namespace Andromeda.Mock
{
    class Perms : Interfaces.IPerms
    {
        public string Version
            => "MockPerms";

        public bool RequestPermission(Entity ent, string perm, out string message)
        {
            message = "Allowed";
            return true;
        }

        public string GetFormattedName(Entity ent)
            => ent.Name;

        public bool IsImmuneTo(Entity target, Entity issuer)
            => false;
    }
}

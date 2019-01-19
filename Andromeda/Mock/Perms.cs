using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;

namespace CommonFunctionality.Mock
{
    class Perms : Interfaces.IPerms
    {
        public string Version
            => "MockPerms";

        public bool HasPermission(Entity ent, string perm)
            => true;

        public string GetFormattedName(Entity ent)
            => ent.Name;

        public bool IsImmuneTo(Entity target, Entity issuer)
            => false;
    }
}

using Andromeda;
using InfinityScript;
using InfinityScript.PBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntiCheat
{
    public static class Utils
    {
        public static void WarnAdminsWithPerm(Entity sender, string perm, IEnumerable<string> message)
        {
            foreach (Entity admin in Common.Perms.PlayersWithPerm(perm))
                if(sender != admin)
                    admin.Tell(message);
        }

        public static void WarnAdminsWithPerm(Entity sender, string perm, string message)
        {
            foreach (Entity admin in Common.Perms.PlayersWithPerm(perm))
                if (sender != admin)
                    admin.Tell(message);
        }
    }
}

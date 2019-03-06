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
        public static IEnumerable<Entity> OnlineAdminsWithPerms(string perm) => BaseScript.Players.Where(x => Common.Perms.RequestPermission(x, perm, out string message));

        public static void WarnAdminsWithPerm(Entity sender, string perm, string message)
        {
            foreach (Entity admin in OnlineAdminsWithPerms(perm))
                if(sender != admin)
                    admin.Tell(message);
        }
    }
}

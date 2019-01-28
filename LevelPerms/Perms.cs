using Andromeda;
using Andromeda.Interfaces;
using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LevelPerms
{
    internal class Perms : IPerms
    {
        public static readonly IPerms Instance = new Perms();

        public string Version
            => "LevelPerms v0.0.1";

        public string[] Credits
            => new[] { "da bois" };

        public string GetFormattedName(Entity entity)
        {
            var lvl = Main.GetLevel(entity);

            if (lvl == 0)
                return entity.Name;

            return $"^7[^2{lvl}^7]{entity.Name}";
        }

        public bool IsImmuneTo(Entity target, Entity issuer)
            => Main.GetLevel(target) >= Main.GetLevel(issuer);

        public bool RequestPermission(Entity entity, string permission, out string message)
        {
            var lvl = Main.GetLevel(entity);

            var reqlvl = Main.GetPermissionLevel(permission);

            if (reqlvl == 0)
            {
                message = "Permission level 0";
                return true;
            }

            if(lvl <= 0)
            {
                message = "Permission denied. Try logging in.";
                return false;
            }

            if(lvl >= reqlvl)
            {
                message = "Level allows permission";
                return true;
            }

            message = $"Required level: {reqlvl}";
            return false;
        }
    }
}

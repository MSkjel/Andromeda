using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Andromeda.Interfaces;
using InfinityScript;
using Andromeda;

namespace GroupPerms
{
    class Perms : IPerms
    {
        public static Perms Instance = new Perms();

        public string Version { get; } = "GroupPerms v1.0.0";

        public string[] Credits { get; } =
        {
            "dem bois"
        };

        public IEnumerable<Entity> PlayersWithDBField(string field) => BaseScript.Players.Where(x => x.GetDBFieldOr(field, "False") == "True");

        public IEnumerable<Entity> PlayersWithPerm(string perm) => BaseScript.Players.Where(x => RequestPermission(x, perm, out string message));

        public string GetFormattedName(Entity entity)
            => entity.GetGroup().FormatName(entity.Name);

        public bool IsImmuneTo(Entity target, Entity issuer)
            => target.RequestPermission($"immuneto.{issuer.GetGroup().Name}", out _);

        public bool RequestPermission(Entity entity, string permission, out string message)
        {
            if(entity.GetGroup().RequestPermission(permission, out message))
                return true;

            if (!entity.IsLogged())
                message = "Permission denied. Try logging in";

            return false;
        }

        public bool IsDefaultLevelOrGroup(Entity ent) =>
            ent.GetGroup(true) == Main.Config.DefaultGroup;

    }
}

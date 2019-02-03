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

        public string GetFormattedName(Entity entity)
            => entity.GetGroup().FormatName(entity.Name);

        public bool IsImmuneTo(Entity target, Entity issuer)
            => target.RequestPermission($"immuneto.{issuer.GetGroup().Name}", out _);

        public bool RequestPermission(Entity entity, string permission, out string message)
            => entity.GetGroup().CanDo(permission, out message);
    }
}

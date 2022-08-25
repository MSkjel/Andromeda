using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;

namespace Andromeda.Interfaces
{
    public interface IPerms : IFunctionality
    {
        bool RequestPermission(Entity entity, string permission, out string message);

        IEnumerable<Entity> PlayersWithDBField(string field);

        IEnumerable<Entity> PlayersWithPerm(string perm);

        string GetFormattedName(Entity entity);

        bool IsImmuneTo(Entity target, Entity issuer);

        bool IsDefaultLevelOrGroup(Entity entity);
    }
}

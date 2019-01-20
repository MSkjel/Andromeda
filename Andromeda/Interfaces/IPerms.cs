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

        string GetFormattedName(Entity entity);

        bool IsImmuneTo(Entity target, Entity issuer);
    }
}

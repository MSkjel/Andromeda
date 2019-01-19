using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;

namespace CommonFunctionality.Interfaces
{
    public interface IPerms : IFunctionality
    {
        bool HasPermission(Entity entity, string permission);
        string GetFormattedName(Entity entity);
        bool IsImmuneTo(Entity target, Entity issuer);
    }
}

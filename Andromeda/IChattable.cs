using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;

namespace Andromeda
{
    public interface IClient
    {
        string Name { get; }
        string GetFormattedName();
        void RawTell(IEnumerable<string> messages);
        void RawSay(IEnumerable<string> messages);
        bool RequestPermission(string permission, out string message);
        bool IsEntity { get; }
        Entity Entity { get; }
    }

    internal class EntityWrapper : IClient
    {
        public string Name
            => Entity.Name;

        public string GetFormattedName()
            => Entity.GetFormattedName();

        public bool IsEntity
            => true;

        public Entity Entity { get; private set; }

        public bool RequestPermission(string permission, out string message)
            => Entity.RequestPermission(permission, out message);

        public void RawTell(IEnumerable<string> messages)
            => Entity.Tell(messages);

        public void RawSay(IEnumerable<string> messages)
            => Entity.Tell(messages);

        public EntityWrapper(Entity ent)
        {
            Entity = ent;
        }
    }

    internal class ConsoleWrapper : IClient
    {
        public string Name
            => Common.Utils.ServerName;

        public bool IsEntity
            => false;

        public Entity Entity
            => null;

        public string GetFormattedName()
            => Name;

        public void RawSay(IEnumerable<string> messages)
        {
            foreach (var msg in messages)
                Log.Info(msg);
        }

        public void RawTell(IEnumerable<string> messages)
        {
            foreach (var msg in messages)
                Log.Info(msg);
        }

        public bool RequestPermission(string permission, out string message)
        {
            message = "Console almighty";
            return true;
        }
    }
}

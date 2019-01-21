using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfinityScript.Events
{
    public class NotifyArgs : EventArgs
    {
        public Entity Entity { get; private set; }
        public int Id { get; private set; }
        public string Notify { get; private set; }
        public Parameter[] Parameters { get; private set; }

        public NotifyArgs(Entity entity, int id, string notify, Parameter[] parameters)
        {
            Entity = entity;
            Id = id;
            Notify = notify;
            Parameters = parameters;
        }
    }
}

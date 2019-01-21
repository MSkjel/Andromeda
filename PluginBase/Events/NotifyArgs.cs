using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfinityScript.Events
{
    public class NotifyArgs : EventArgs
    {
        private readonly int id;
        private Entity ent;
        public Entity Entity
        {
            get
            {
                ent = ent ?? Entity.GetEntity(id);
                return ent;
            }
        }

        public string Notify { get; private set; }
        public Parameter[] Parameters { get; private set; }

        public NotifyArgs(int id, string notify, Parameter[] parameters)
        {
            this.id = id;
            Notify = notify;
            Parameters = parameters;
        }
    }
}

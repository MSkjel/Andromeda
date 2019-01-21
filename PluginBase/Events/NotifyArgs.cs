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

        public Entity EntityParam => Parameters[0].As<Entity>();

        //public Entity Player
        //{
        //    get
        //    {
        //        if (Entity != null && Entity.IsPlayer)
        //            return Entity;

        //        if (Parameters[0].Type == VariableType.Entity)
        //        {
        //            Entity player = Parameters[0].As<Entity>();

        //            if (player.IsPlayer)
        //                return player;

        //            throw new NullReferenceException($"EntID: {player.EntRef} is not a player");
        //        }
                
        //        throw new NullReferenceException($"Parameter[0] is not an Entity");
        //    }
        //}

        public string Notify { get; private set; }
        public Parameter[] Parameters { get; private set; }

        public NotifyArgs(int id, string notify, Parameter[] parameters)
        {
            this.id = id;
            Notify = notify;
            Parameters = parameters;
        }

        public void Deconstruct(out Entity player, out string notify, out Parameter[] parameters)
        {
            player = Entity;
            notify = Notify;
            parameters = Parameters;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;

namespace Andromeda.Mock
{
    class Admin : Interfaces.IAdmin
    {
        public string Version
            => "MockAdmin";
        public void Kick(Entity player, string issuer, string message)
            => throw new NotImplementedException();

        public void Ban(Entity player, string issuer, string message)
            => throw new NotImplementedException();

        public void TempBan(Entity ent, string issuer, string message)
            => throw new NotImplementedException();

        public void TempBan(Entity ent, string issuer, TimeSpan timeSpan, string message)
            => throw new NotImplementedException();
    }
}

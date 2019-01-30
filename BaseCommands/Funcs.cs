using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;

namespace BaseAdmin
{
    public static class Funcs
    {
        public static void Ban(Entity ent, string issuer, string message = "You have been banned")
        {
            throw new NotImplementedException();
        }

        public static void Kick(Entity ent, string issuer, string message = "You have been kicked")
        {
            throw new NotImplementedException();
        }

        public static void TempBan(Entity ent, string issuer, string message = "You have been temporarily banned")
        {
            throw new NotImplementedException();
        }

        public static void TempBan(Entity ent, string issuer, TimeSpan timeSpan, string message = "You have been temporarily banned")
        {
            throw new NotImplementedException();
        }

        public static void Unwarn(Entity ent, string issuer, string reason = "You have been unwarned")
        {
            throw new NotImplementedException();
        }

        public static void Warn(Entity ent, string issuer, string reason = "You have been warned")
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;

namespace Andromeda.Interfaces
{
    public interface IAdmin : IFunctionality
    {
        void Warn(Entity ent, string issuer, string reason = "You have been warned");

        void Unwarn(Entity ent, string issuer, string reason = "You have been unwarned");

        void ResetWarnings(Entity ent, string issuer, string reason = "Your warnings have been reset");

        void TempBan(Entity ent, string issuer, string message = "You have been temporarily banned");

        void TempBan(Entity ent, string issuer, TimeSpan timeSpan, string message = "You have been temporarily banned");

        void Kick(Entity ent, string issuer, string message = "You have been kicked");

        void Ban(Entity ent, string issuer, string message = "You have been banned");
    }
}

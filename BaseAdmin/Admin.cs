using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Andromeda.Interfaces;
using InfinityScript;

namespace BaseAdmin
{
    public class Admin : IAdmin
    {
        public static IAdmin Instance = new Admin();

        public string Version { get; } = "BaseAdmin v0.0.1";

        public string[] Credits { get; } = new[]
        {
            "dem bois"
        };

        public void Ban(Entity ent, string issuer, string message = "You have been banned")
            => Funcs.Ban(ent, issuer, message);

        public void Kick(Entity ent, string issuer, string message = "You have been kicked")
            => Funcs.Kick(ent, issuer, message);

        public void ResetWarnings(Entity ent, string issuer, string reason = "Your warnings have been reset")
            => Funcs.ResetWarnings(ent, issuer, reason);

        public void TempBan(Entity ent, string issuer, string message = "You have been temporarily banned")
            => Funcs.TempBan(ent, issuer, TimeSpan.FromMinutes(20), message);

        public void TempBan(Entity ent, string issuer, TimeSpan timeSpan, string message = "You have been temporarily banned")
            => Funcs.TempBan(ent, issuer, timeSpan, message);

        public void Unwarn(Entity ent, string issuer, string reason = "You have been unwarned")
            => Funcs.Unwarn(ent, issuer, reason);

        public void Warn(Entity ent, string issuer, string reason = "You have been warned")
            => Funcs.Warn(ent, issuer, reason);
    }
}

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Andromeda;
using Andromeda.Interfaces;
using InfinityScript;

namespace ServerUtils
{
    public class Utils : IUtils
    {
        public static IUtils Instance;

        public ColorScheme ColorScheme { get; } = new ColorScheme(
            normal: Main.Config.Colors["normal"],
            info: Main.Config.Colors["info"],
            error: Main.Config.Colors["error"],
            admin: Main.Config.Colors["admin"],
            player: Main.Config.Colors["player"],

            highlight1: Main.Config.Colors["highlight1"],
            highlight2: Main.Config.Colors["highlight2"],
            highlight3: Main.Config.Colors["highlight3"],
            highlight4: Main.Config.Colors["highlight4"]);

        public string ServerName
            => Main.Config.ServerDisplayName;

        public string Version { get; } = "ServerUtils v1.0.0";

        public string[] Credits { get; } = new[]
        {
            "dem bois"
        };

        public void SayAllPlayers(IEnumerable<string> messages)
        {
            if (!messages.Any())
                return;

            IEnumerator routine()
            {
                using (var e = messages.GetEnumerator())
                {
                    if (e.MoveNext())
                        Utilities.RawSayAll($"{Main.Config.PublicPrefix}{e.Current}");

                    while (e.MoveNext())
                    {
                        yield return BaseScript.Wait(Main.Config.MessageTrailDelay);

                        Utilities.RawSayAll($"{Main.Config.PublicTrail}{e.Current}");
                    }
                }
            }

            Async.Start(routine());
        }

        public void SayToPlayer(Entity player, IEnumerable<string> messages)
        {
            if (!messages.Any())
                return;

            IEnumerator routine()
            {
                using (var e = messages.GetEnumerator())
                {
                    if (e.MoveNext())
                        Utilities.RawSayTo(player, $"{Main.Config.PrivatePrefix}{e.Current}");

                    while (e.MoveNext())
                    {
                        yield return BaseScript.Wait(Main.Config.MessageTrailDelay);

                        Utilities.RawSayTo(player, $"{Main.Config.PrivateTrail}{e.Current}");
                    }
                }
            }

            Async.Start(routine());
        }
    }
}

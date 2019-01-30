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
        public static IUtils Instance = new Utils();

        public ColorScheme ColorScheme
            => Config.Instance.ColorScheme;

        public string ServerName
            => Config.Instance.ServerDisplayName;

        public string Version { get; } = "ServerUtils v1.0.0";

        public string[] Credits { get; } = new[]
        {
            "dem bois"
        };

        public void SayAll(IEnumerable<string> messages)
        {
            if (!messages.Any())
                return;

            IEnumerator routine()
            {
                using (var e = messages.GetEnumerator())
                {
                    if (e.MoveNext())
                        Utilities.RawSayAll($"{Config.Instance.PublicPrefix}{e.Current}");

                    while (e.MoveNext())
                    {
                        yield return BaseScript.Wait(Config.Instance.MessageTrailDelay);

                        Utilities.RawSayAll($"{Config.Instance.PublicTrail}{e.Current}");
                    }
                }
            }

            Async.Start(routine());
        }

        public void SayTo(Entity player, IEnumerable<string> messages)
        {
            if (!messages.Any())
                return;

            IEnumerator routine()
            {
                using (var e = messages.GetEnumerator())
                {
                    if (e.MoveNext())
                        Utilities.RawSayTo(player, $"{Config.Instance.PrivatePrefix}{e.Current}");

                    while (e.MoveNext())
                    {
                        yield return BaseScript.Wait(Config.Instance.MessageTrailDelay);

                        Utilities.RawSayTo(player, $"{Config.Instance.PrivateTrail}{e.Current}");
                    }
                }
            }

            Async.Start(routine());
        }
    }
}

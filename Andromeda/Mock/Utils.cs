using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using InfinityScript;

namespace Andromeda.Mock
{
    public class Utils : Interfaces.IUtils
    {
        public string Version
            => "MockUtils";
        public string[] Credits
            => new string[0];


        public ColorScheme ColorScheme { get; } = new ColorScheme(
            normal: "^7",
            info: "^6",
            error: "^1",
            admin: "^3",
            player: "^;",
            
            highlight1: "^:",
            highlight2: "^2",
            highlight3: "^8",
            highlight4: "^5");

        public void SayAll(IEnumerable<string> messages)
        {
            if (!messages.Any())
                return;

            IEnumerator routine()
            {
                using (var e = messages.GetEnumerator())
                {
                    if (e.MoveNext())
                        Utilities.RawSayAll($"[Server] {e.Current}");

                    while (e.MoveNext())
                    {
                        yield return BaseScript.Wait(0.85f);

                        Utilities.RawSayAll($"* {e.Current}");
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
                        Utilities.RawSayTo(player, $"[PM] {e.Current}");

                    while(e.MoveNext())
                    {
                        yield return BaseScript.Wait(0.85f);

                        Utilities.RawSayTo(player, $"> {e.Current}");
                    }
                }
            }

            Async.Start(routine());
        }
    }
}

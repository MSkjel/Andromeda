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
            admin: "^4",

            highlight1: "^2",
            highlight2: "^3",
            highlight3: "^5",
            highlight4: "^;",
            highlight5: "^:");

        public void SayAll(IEnumerable<string> messages)
        {
            if (!messages.Any())
                return;

            IEnumerator routine()
            {
                foreach(var message in messages)
                {
                    Utilities.RawSayAll($"[Server] {message}");

                    yield return BaseScript.Wait(0.7f);
                }
            }

            BaseScript.StartAsync(routine());
        }

        public void SayTo(Entity player, IEnumerable<string> messages)
        {
            if (!messages.Any())
                return;

            IEnumerator routine()
            {
                foreach (var message in messages)
                {
                    Utilities.RawSayTo(player, $"[PM] {message}");

                    yield return BaseScript.Wait(0.7f);
                }
            }

            BaseScript.StartAsync(routine());
        }
    }
}

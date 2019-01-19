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

        public void SayAll(IEnumerable<Msg> messages)
        {
            if (!messages.Any())
                return;

            IEnumerator routine()
            {
                foreach(var message in messages)
                {
                    Utilities.RawSayAll($"[Server] {translate(message)}");

                    yield return BaseScript.Wait(0.7f);
                }
            }

            BaseScript.StartAsync(routine());
        }

        public void SayTo(Entity player, IEnumerable<Msg> messages)
        {
            if (!messages.Any())
                return;

            IEnumerator routine()
            {
                foreach (var message in messages)
                {
                    Utilities.RawSayTo(player, $"[PM] {translate(message)}");

                    yield return BaseScript.Wait(0.7f);
                }
            }

            BaseScript.StartAsync(routine());
        }

        private static string translate(Msg message)
        {
            switch(message.Type)
            {
                case MsgType.Info:
                    return $"^3{message}";
                case MsgType.Extra:
                    return $"^7{message}";
                case MsgType.Error:
                    return $"^1{message}";
                case MsgType.Admin:
                    return $"^5{message}";
            }

            return message.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using InfinityScript;
using Andromeda;
using Andromeda.Parse;

namespace ServerUtils
{
    [Plugin]
    public class Main
    {
        [EntryPoint]
        private static void Init()
        {
            IEnumerator announcer()
            {
                GSCFunctions.SetDvarIfUninitialized("announcer.index", 0);

                var index = GSCFunctions.GetDvarInt("announcer.index");
                
                while(true)
                {
                    index = index % Config.Instance.Announcements.Length;

                    Common.SayAll(Config.Instance.Announcements[index]);
                    index++;

                    yield return BaseScript.Wait(Config.Instance.AnnounceInterval);
                }
            }

            Async.Start(announcer());

            foreach(var cmd in Config.Instance.InfoCommands)
            {
                // INFO COMMANDS
                Command.TryRegister(SmartParse.CreateCommand(
                    name: cmd.Key,
                    argTypes: null,
                    action: delegate (Entity sender, object[] args)
                    {
                        sender.Tell(cmd.Value);
                    },
                    usage: $"!{cmd.Key}",
                    description: $"Shows information regarding {cmd.Key}"));
            }
        }

        static Main()
        {
            Config.Load();

            Common.Register(Utils.Instance);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfinityScript;
using Andromeda.Events;

namespace AdvancedAdmin
{
    [Plugin]
    public class Main
    {
        [EntryPoint]
        private static void Init()
        {
            Events.DSRLoad.Add((sender, args) =>
            {
                if(args.TryGetOpt("custom_gametype", out var value))
                    ServerStr.ReportedGameType = value;
            });
        }
    }
}

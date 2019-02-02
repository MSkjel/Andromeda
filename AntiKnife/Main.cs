using Andromeda;
using Andromeda.Events;
using Andromeda.Parse;
using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntiKnife
{
    [Plugin]
    public static class Main
    {
        private static bool parseBool(string val)
            => val != "0";

        [EntryPoint]
        private static void Init()
        {
            AntiKnife knife = new AntiKnife();

            knife.SetupKnife();

            Events.DSRLoad.Add((sender, args) =>
            {
                if (args.TryGetOpt("antiknife.enable", out var enabled) && parseBool(enabled))
                    knife.DisableKnife();
            });

            Command.TryRegister(SmartParse.CreateCommand(
                name: "setknife",
                argTypes: new[] { SmartParse.Boolean },
                action: delegate (Entity sender, object[] args)
                {
                    bool state = (bool)args[0];

                    if (state)
                        knife.EnableKnife();
                    else
                        knife.DisableKnife();

                    sender.Tell($"%nKnife set to %i{state}");
                },
                usage: "!setknife <state>",
                description: "Enables or disables knife"));
        }
    }
}

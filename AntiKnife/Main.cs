﻿using Andromeda;
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

        [EntryPoint]
        private static void Init()
        {
            Events.DSRLoad.Add((sender, args) =>
            {
                Log.Debug("Starting AntiKnife");

                if (args.GetBoolOrDefault("antiknife.enable", false))
                {
                    Log.Debug("Knife Disabled");
                    AntiKnife.DisableKnife();
                }
                else
                {
                    Log.Debug("Knife Enabled");
                    AntiKnife.EnableKnife();
                }

                Log.Debug("Finished AntiKnife");
            });

            // SETKNIFE
            Command.TryRegister(SmartParse.CreateCommand(
                name: "setknife",
                argTypes: new[] { SmartParse.Boolean },
                action: delegate (IClient sender, object[] args)
                {
                    bool state = (bool)args[0];

                    if (state)
                        AntiKnife.EnableKnife();
                    else
                        AntiKnife.DisableKnife();

                    sender.Tell($"%nKnife set to %i{state}");
                },
                usage: "!setknife <state>",
                description: "Enables or disables knife",
                permission: "setknife"));
        }
    }
}

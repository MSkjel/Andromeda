using Andromeda;
using Andromeda.Events;
using Andromeda.Parse;
using InfinityScript;
using InfinityScript.PBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace BaseCommands
{
    [Plugin]
    public class Base
    {
        static Base()
        {
            #region Commands
            #region Map
            // MAP
            Command.TryRegister(SmartParse.CreateCommand(
                name: "map",
                argTypes: new[] { Parse.GameMap.Obj },
                action: delegate (Entity sender, object[] args)
                {
                    var map = args[0] as GameMap;

                    Common.SayAll($"Map has been changed to %h1{map.NiceName} %nby %p{sender.GetFormattedName()}");

                    Utilities.ExecuteCommand($"map {map.RawName}");
                },
                usage: "!map <map>",
                permission: "map",
                description: "Changes the map to the map specified"));

            // MAPS
            Command.TryRegister(SmartParse.CreateCommand(
                name: "maps",
                argTypes: null,
                action: delegate (Entity sender, object[] args)
                {
                    var msgs = "%iAvailable maps:".Yield()
                        .Concat(Common.Condense(
                            Utils.Maps.Where(x => Utils.MapFilesExist(x.RawName))
                            .Select(x => $"%a{x.NiceName}%n"))
                            );

                    sender.Tell(msgs);
                },
                usage: "!maps",
                permission: "maps",
                description: "Displays the available maps"));

            // SETNEXTMAP (incomplete)
            Command.TryRegister(SmartParse.CreateCommand(
                name: "setnextmap",
                argTypes: new[] { Parse.GameMap.Obj },
                action: delegate (Entity sender, object[] args)
                {
                    var map = args[0] as GameMap;

                    DSR.SetNextMap(map.RawName);

                    Common.SayAll($"%p{sender.GetFormattedName()} %nhas set next map to %i{map.NiceName}%n.");
                },
                usage: "!setnextmap <map>",
                permission: "setnextmap",
                description: "Sets the next map to the map specified"));
            #endregion

            // EXECUTECOMMAND
            Command.TryRegister(SmartParse.CreateCommand(
                name: "executecommand",
                argTypes: new[] { SmartParse.GreedyString },
                action: delegate (Entity sender, object[] args)
                {
                    var cmd = args[0] as string;

                    Utilities.ExecuteCommand(cmd);

                    sender.Tell("%aCommand executed.");
                },
                usage: "!executecommand <cmd>",
                aliases: new[] { "exec" },
                permission: "executecommand",
                description: "Executes a command in the server console"));

            #endregion
        }

        [EntryPoint]
        private static void Init()
        {
        }
    }
}

using Andromeda;
using Andromeda.Cmd;
using InfinityScript;
using InfinityScript.PBase;
using System;
using System.Collections.Generic;
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
            Command.TryRegister(SmartParse.CreateCommand(
                name: "map",
                argTypes: new[] { Parse.GameMap.Obj },
                action: delegate (Entity sender, object[] args)
                {
                    var map = args[0] as GameMap;

                    Common.SayAll($"Map has been changed to %h2{map.NiceName} %nby %h1{Common.Perms.GetFormattedName(sender)}");

                    Utilities.ExecuteCommand($"map {map.RawName}");
                },
                usage: "!map <map>",
                permission: "map",
                description: "Changes the map to the map specified"));

            Command.TryRegister(SmartParse.CreateCommand(
                name: "maps",
                argTypes: null,
                action: delegate (Entity sender, object[] args)
                {
                    var msgs = "%iAvailable maps:".Yield()
                        .Concat(Common.Condense(
                            Utils.Maps.Where(x => Utils.MapFilesExist(x.RawName))
                            .Select(x => $"%a{x.NiceName}"))
                            );

                    sender.Tell(msgs);
                },
                usage: "!maps",
                permission: "maps",
                description: "Displays the available maps"));

            Command.TryRegister(SmartParse.CreateCommand(
                name: "setnextmap",
                argTypes: new[] { SmartParse.String },
                action: delegate (Entity sender, object[] args)
                {

                },
                usage: "!setnextmap <map>",
                permission: "setnextmap",
                description: "Sets the next map to the map specified"));
            #endregion
            #endregion
        }

        [EntryPoint]
        private void Init()
        {

        }
    }
}

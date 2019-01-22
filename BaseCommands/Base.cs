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
                argTypes: new[] { SmartParse.String },
                action: delegate (Entity sender, object[] args)
                {
                    var query = Utils.Maps.Where(m => m.Matches(args[0] as string));

                    if (query.Count() == 1)
                    {
                        var map = query.FirstOrDefault();

                        if (Utils.MapFilesExist(map.RawName))
                        {
                            Common.Utils.SayAll(new Msg[]
                            {
                                $"%nMap has been changed to %h2{map.NiceName} %nby %h1{Common.Perms.GetFormattedName(sender)}"
                            });

                            Utilities.ExecuteCommand($"map {map.RawName}");
                        }
                        else
                            sender.Tell($"%eThe server does not have %h2{map.NiceName} %einstalled");
                    }
                    else
                        sender.Tell("%eNo or more maps found");
                },
                usage: "!map <map>",
                permission: "map",
                description: "Changes the map to the map specified"));

            Command.TryRegister(SmartParse.CreateCommand(
               name: "maps",
               argTypes: null,
               action: delegate (Entity sender, object[] args)
               {
                   sender.Tell("%nAvailable maps:");
                   Utils.PMOnInterval(sender, Utils.Maps.Where(x => Utils.MapFilesExist(x.RawName)).Select(x => $"%a{x.NiceName}"), 850);
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
    }
}

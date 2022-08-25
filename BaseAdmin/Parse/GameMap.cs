using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Andromeda.Parse;
using Andromeda;
using InfinityScript;

namespace BaseAdmin.Parse
{
    class GameMap : IArgParse
    {
        public static readonly IArgParse Obj = new GameMap();

        public string Parse(ref string str, out object parsed, IClient sender)
        {
            if (SmartParse.String.Parse(ref str, out parsed, sender) != null)
                return "Expected map name";

            var mapname = parsed as string;
            var query = Utils.Maps.Value.Where(m => m.Matches(mapname));

            if (!query.Any())
                return "No maps found. Do !maps";

            if(query.Count() == 1)
            {
                var first = query.FirstOrDefault();

                if (!Utils.MapFilesExist(first.RawName))
                    return $"Map {first.NiceName} is not installed on the server";

                parsed = first;
                return null;
            }

            return "More than one map found";
        }
    }
}

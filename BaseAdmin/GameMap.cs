﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseAdmin
{
    public class GameMap
    {
        private readonly List<string> KnownNames = new List<string>();

        public GameMap(string mapname, string nicename, params string[] aliases)
        {
            KnownNames.Add(mapname);
            KnownNames.Add(nicename);
            KnownNames.AddRange(aliases);
        }

        public string RawName => KnownNames[0];

        public string NiceName => KnownNames[1];

        public bool Matches(string identifier)
        {
            foreach (var name in KnownNames)
                if (Utils.CaseInsensitiveStartsWith(name, identifier))
                    return true;

            return false;
        }
    }
}

using Andromeda;
using Andromeda.Parse;
using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseAdmin.Parse
{
    class GameMode : IArgParse
    {
        public static readonly IArgParse Obj = new GameMode();

        public string Parse(ref string str, out object parsed, IClient sender)
        {
            if (SmartParse.String.Parse(ref str, out parsed, sender) != null)
                return "Expected dsr name";

            if (DSR.DSRExists(parsed as string))
            {
                parsed = DSR.GetFullDSRName(parsed as string);
                return null;
            }

            return "No DSR found. Do !modes";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Andromeda.Events.EventArguments
{
    public class DSRLoadArgs : EventArgs
    {
        public Dictionary<string, string> DSROptions { get; internal set; }

        public DSRLoadArgs(Dictionary<string, string> dict)
        {
            DSROptions = dict;
        }

        public void Deconstruct(out Dictionary<string,string> dict)
        {
            dict = DSROptions;
        }
    }
}

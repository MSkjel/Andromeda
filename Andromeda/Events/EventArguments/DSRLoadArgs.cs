using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Andromeda.Events.EventArguments
{
    public class DSRLoadArgs : EventArgs
    {
        private readonly Dictionary<string, string> DSROptions;

        public DSRLoadArgs(Dictionary<string, string> dict)
        {
            DSROptions = dict;
        }

        public string this[string str]
            => DSROptions[str];

        public bool TryGetOpt(string opt, out string value)
            => DSROptions.TryGetValue(opt, out value);

        public string GetOrDefault(string opt, string def = default)
        {
            if (TryGetOpt(opt, out var val))
                return val;

            return def;
        }

        public bool GetBoolOrDefault(string opt, bool def = default)
        {
            if (TryGetOpt(opt, out var val))
                return val != "0";

            return def;
        }

        public void Deconstruct(out Dictionary<string,string> dict)
        {
            dict = DSROptions.ToDictionary(v => v.Key, v => v.Value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Andromeda.Parse;
using InfinityScript;

namespace BaseAdmin.Parse
{
    public class TimeSpan : IArgParse
    {
        public static IArgParse Obj = new TimeSpan();

        public string Parse(ref string str, out object parsed, Entity sender)
        {
            int parseFrom(string s)
            {
                if (!string.IsNullOrEmpty(s) && int.TryParse(s, out var ret))
                    return ret;

                return 0;
            }

            var match = Regex.Match(str, @"^(?:(\d+)d)?(?:(\d+)h)?(?:(\d)+m)?(?:\s+(.+))?");

            if(match.Success)
            {
                var timeSpan = new System.TimeSpan(
                    parseFrom(match.Groups[1].Value),
                    parseFrom(match.Groups[2].Value),
                    parseFrom(match.Groups[3].Value),
                    0);

                str = match.Groups[4].Value;

                parsed = timeSpan;
                return null;
            }

            parsed = null;
            return null;
        }
    }
}

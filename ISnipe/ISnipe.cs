using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Andromeda.Interfaces;

namespace ISnipe
{
    internal class ISnipe : IFunctionality
    {
        public static ISnipe Instance = new ISnipe();
        public string Version { get; } = "ISnipe v1.0.0";

        public string[] Credits { get; } = new[]
        {
            "dem bois"
        };
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Andromeda.Interfaces;

namespace AntiCheat
{
    public class AntiCheat : IFunctionality
    {
        internal static readonly AntiCheat Instance = new AntiCheat();

        public string Version
            => "AntiCheat v0.0.1";

        public string[] Credits { get; } = new[]
        {
            "Markus"
        };
    }
}

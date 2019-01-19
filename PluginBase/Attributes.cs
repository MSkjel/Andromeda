using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfinityScript
{
    [AttributeUsage(AttributeTargets.Method,
                   AllowMultiple = false,
                   Inherited = false)]
    public class EntryPointAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class,
               AllowMultiple = false,
               Inherited = false)]
    public class PluginAttribute : Attribute
    {
    }

}

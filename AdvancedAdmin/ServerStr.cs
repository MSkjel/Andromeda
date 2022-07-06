using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace AdvancedAdmin
{
    static class ServerStr
    {
        public static string ReportedMapName
        {
            get => Get("m");
            set => Set("m", value);
        }

        public static string ReportedGameType
        {
            get => Get("gt");
            set => Set("gt", value);
        }

        internal static string Get(string index)
        {
            string[] str = Marshal.PtrToStringAnsi(ptr).Split('\\');

            for (int i = 0; i < str.Length - 1; i++)
            {
                if (str[i] == index && i % 2 == 0)
                    return str[i + 1];
            }

            return null;
        }

        internal static void Set(string index, string value)
        {
            value = value.Replace(@"\", "");

            string[] str = Marshal.PtrToStringAnsi(ptr).Split('\\');
            for (int i = 0; i < str.Length - 1; i++)
            {
                if (str[i] == index && i % 2 == 0)
                    str[i + 1] = value;
            }

            var newstr = string.Join(@"\", str);

            WriteStringASCII(ptr, newstr);
        }

        private static ProcessModule GetModule(Process process, string ModuleName)
        {
            foreach (ProcessModule module in process.Modules)
                if (module.ModuleName == ModuleName)
                    return module;

            return null;
        }

        private static void WriteStringASCII(IntPtr Address, string Value)
        {
            byte[] asciipls = Encoding.ASCII.GetBytes(Value);
            Marshal.Copy(asciipls, 0, Address, asciipls.Length);
            Marshal.WriteByte(Address + asciipls.Length, 0);
        }

        private static readonly IntPtr ptr;

        static ServerStr()
        {
            ptr = Marshal.ReadIntPtr(GetModule(Process.GetCurrentProcess(), "TeknoMW3S.dll").BaseAddress + 0x0007037C) + 0x854 + 0x1;
        }
    }
}

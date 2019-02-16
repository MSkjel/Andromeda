﻿using System;
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
            string[] str = Marshal.PtrToStringAnsi(ptr).Split(new[] { @"\" }, StringSplitOptions.None);

            for (int i = 0; i < str.Length - 1; i++)
            {
                if (str[i] == "m" && i % 2 == 0)
                    return str[i + 1];
            }

            return null;
        }

        internal static void Set(string index, string value)
        {
            value = value.Replace(@"\", "");

            var oldstr = Marshal.PtrToStringAnsi(ptr);

            InfinityScript.Log.Info($"Setting {value} at {index}");
            InfinityScript.Log.Info($"Old serverstr: {oldstr}");

            string[] str = oldstr.Split('\\');
            for (int i = 0; i < str.Length - 1; i++)
            {
                if (str[i] == index && i % 2 == 0)
                    str[i + 1] = value;
            }

            var newstr = string.Join(@"\", str);

            InfinityScript.Log.Info($"New serverstr: {newstr}");

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
            ptr = Marshal.ReadIntPtr(GetModule(Process.GetCurrentProcess(), "TeknoMW3S.dll").BaseAddress + 0x00062C9C) + 0x74 + 0x7E1;
        }
    }
}

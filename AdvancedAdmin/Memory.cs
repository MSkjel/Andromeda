using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedAdmin
{
    internal class Memory
    {
        internal static readonly IntPtr ClantagAddress = (IntPtr)0x01AC5564;
        internal static readonly IntPtr TitleAddress = (IntPtr)0x01AC5548;
        internal static readonly IntPtr NameAddress = (IntPtr)0x01AC5508;
        internal static readonly IntPtr ClassNameAddress = (IntPtr)0x04A2E39D;
        internal static readonly IntPtr UseClanTagAddress = (IntPtr)0x01AC5540;
        internal static readonly IntPtr UseCustomTitleAddress = (IntPtr)0x01AC5544;

        internal static readonly int PlayerDataSize2 = 0x38A4;
        internal static readonly int PlayerClassNameDataSize = 0x78688;
        internal static readonly int ClassNameDataSize = 0x62;

        internal static IntPtr CalculateClantagAddress(int EntRef) => ClantagAddress + EntRef * PlayerDataSize2;

        internal static IntPtr CalculateTitleAddress(int EntRef) => TitleAddress + EntRef * PlayerDataSize2;

        internal static IntPtr CalculateNameAddress(int EntRef) => NameAddress + EntRef * PlayerDataSize2;

        internal static IntPtr CalculateUseClanTagAddress(int EntRef) => UseClanTagAddress + EntRef * PlayerDataSize2;

        internal static IntPtr CalculateUseCustomTitleAddress(int EntRef) => UseCustomTitleAddress + EntRef * PlayerDataSize2;

        internal static IntPtr CalculateClassNameAddress(int EntRef) => ClassNameAddress + EntRef * PlayerClassNameDataSize;
    }
}

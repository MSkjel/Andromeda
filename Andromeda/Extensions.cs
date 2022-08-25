using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Andromeda
{
    public static class Extensions
    {
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }


        public static IEnumerable<T> Append<T>(this IEnumerable<T> enumerable, T item)
            => enumerable.Concat(item.Yield());

        public static IEnumerable<T> Append<T>(this T item, IEnumerable<T> enumerable)
            => item.Yield().Concat(enumerable);
        public static IEnumerable<T> Append<T>(this T item1, T item2)
        {
            yield return item1;
            yield return item2;
        }


        public static string Format(this string str, Dictionary<string, string> format)
        {
            var builder = new StringBuilder(str);

            foreach (var val in format)
                builder.Replace(val.Key, val.Value);

            return builder.ToString();
        }

        public static IEnumerable<string> Condense(this IEnumerable<string> strings, int condenseLevel = 40, string separator = ", ")
        {
            var sb = new StringBuilder();
            int sbLength = 0;

            int sepLength = separator.ColorlessLength();

            foreach (var str in strings)
            {
                var strLength = str.ColorlessLength();

                if (sbLength == 0)
                {
                    sb.Append(str);
                    sbLength += strLength;
                    continue;
                }

                if (sb.Length + sepLength + strLength <= condenseLevel)
                {
                    sb.Append(separator);
                    sb.Append(str);

                    sbLength += sepLength + strLength;
                    continue;
                }

                yield return sb.ToString();
                sb.Clear();
                sbLength = 0;
            }

            if(sbLength != 0)
                yield return sb.ToString();

            sb.Clear();
        }

        public static int RealPing(this Entity ent) => Marshal.ReadInt32(new IntPtr(0x04A0CB08) + ent.EntRef * 0x78688);
    }
}

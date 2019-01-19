using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonFunctionality
{
    public struct Msg
    {
        public readonly string String;
        public readonly MsgType Type;

        private Msg(string str, MsgType type)
        {
            String = str;
            Type = type;
        }

        public override string ToString()
            => String;

        public static Msg Info(string str)
            => new Msg(str, MsgType.Info);

        public static Msg Error(string str)
            => new Msg(str, MsgType.Error);

        public static Msg Admin(string str)
            => new Msg(str, MsgType.Admin);

        public static Msg Extra(string str)
            => new Msg(str, MsgType.Extra);

        public static implicit operator Msg(string str)
            => Info(str);
    }

    public enum MsgType
    {
        Info,
        Extra,
        Error,
        Admin
    }
}

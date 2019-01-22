using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseCommands
{
    public static partial class EntityExtensions
    {
        public static T GetFieldOrDef<T>(this Entity ent, string field)
        {
            try
            {
                return ent.GetField<T>(field);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static T GetFieldOrVal<T>(this Entity ent, string field, T def)
        {
            try
            {
                return ent.GetField<T>(field);
            }
            catch (Exception)
            {
                return def;
            }
        }

        public static void SetFieldT(this Entity ent, string field, object args)
        {
            if (args is int)
                ent.SetField(field, new Parameter((int)args));
            else if (args is float)
                ent.SetField(field, new Parameter((float)args));
            else if (args is string)
                ent.SetField(field, new Parameter((string)args));
            else if (args is Vector3)
                ent.SetField(field, new Parameter((Vector3)args));
            else if (args is Entity)
                ent.SetField(field, new Parameter((Entity)args));
            else
                ent.SetField(field, new Parameter(args));
        }

        public static T GetField<T>(this Entity ent, string field) => ent.GetField<T>(field);
        public static bool HasField(this Entity ent, string field) => ent.HasField(field);
        public static void SetField(this Entity ent, string field, Parameter args) => ent.SetField(field, args);
        public static object GetField(this Entity ent, string field) => ent.GetField<object>(field);
        public static bool IsFieldTrue(this Entity ent, string field) => ent.HasField(field) && ent.GetField<int>(field) != 0;
        public static bool IsFieldEqual(this Entity ent, string field, int limit) => ent.HasField(field) && ent.GetField<int>(field) == limit;
        public static bool IsFieldHigherOrEqual(this Entity ent, string field, int limit) => ent.HasField(field) && ent.GetField<int>(field) >= limit;
        public static bool IsFieldHigher(this Entity ent, string field, int limit) => ent.HasField(field) && ent.GetField<int>(field) > limit;
        public static bool IsFieldLowerOrEqual(this Entity ent, string field, int limit) => ent.HasField(field) && ent.GetField<int>(field) <= limit;
        public static bool IsFieldLower(this Entity ent, string field, int limit) => ent.HasField(field) && ent.GetField<int>(field) < limit;

        public static int IncrementField(this Entity ent, string field, int amount)
        {
            ent.SetField(field, ent.GetFieldOrDef<int>(field) + amount);

            return ent.GetField<int>(field);
        }

        public static int DecrementField(this Entity ent, string field, int amount)
        {
            int val = ent.GetFieldOrDef<int>(field) - amount;
            ent.SetField(field, val < 0 ? 0 : val);

            return ent.GetField<int>(field);
        }
    }
}

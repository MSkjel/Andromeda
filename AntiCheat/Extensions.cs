using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntiCheat
{
    public static class Extensions
    {
        public static string CurrentWeaponClass(this Entity ent) => GSCFunctions.WeaponClass(ent.CurrentWeapon);

        public static float NormalizeAngle(float angle)
        {
            return ((angle % 360f) + 360f) % 360f;
        }

        public static float Difference(float a1, float a2)
        {
            return 180 - Math.Abs(Math.Abs(a1 - a2 + 360) - 180);
        }

        public static double DistanceToAngle(this Vector3 angle1, Vector3 angle2)
        {
            return Math.Sqrt(Math.Pow(Difference(angle1.X, angle2.X), 2) + Math.Pow(Difference(angle1.Y, angle2.Y), 2));
        }
    }
}

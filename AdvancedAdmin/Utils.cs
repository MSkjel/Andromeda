using Andromeda;
using InfinityScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedAdmin
{
    public static class Utils
    {
        internal static void InitializeFly(Entity entity)
        {
            entity.NotifyOnPlayerCommand("upPressed", "+gostand");
            entity.NotifyOnPlayerCommand("upReleased", "-gostand");

            entity.NotifyOnPlayerCommand("downPressed", "+breath_sprint");
            entity.NotifyOnPlayerCommand("downReleased", "-breath_sprint");

            entity.NotifyOnPlayerCommand("forwardPressed", "+forward");
            entity.NotifyOnPlayerCommand("forwardReleased", "-forward");

            entity.NotifyOnPlayerCommand("backwardPressed", "+back");
            entity.NotifyOnPlayerCommand("backwardReleased", "-back");

            entity.NotifyOnPlayerCommand("leftPressed", "+moveleft");
            entity.NotifyOnPlayerCommand("leftReleased", "-moveleft");

            entity.NotifyOnPlayerCommand("rightPressed", "+moveright");
            entity.NotifyOnPlayerCommand("rightReleased", "-moveright");

            entity.OnNotify("upPressed", (ent) => entity.SetField("Up", 1));
            entity.OnNotify("downPressed", (ent) => entity.SetField("Down", 1));

            entity.OnNotify("forwardPressed", (ent) => entity.SetField("Forward", 1));
            entity.OnNotify("backwardPressed", (ent) => entity.SetField("Backward", 1));
            entity.OnNotify("leftPressed", (ent) => entity.SetField("Left", 1));
            entity.OnNotify("rightPressed", (ent) => entity.SetField("Right", 1));

            entity.OnNotify("upReleased", (ent) => entity.SetField("Up", 0));
            entity.OnNotify("downReleased", (ent) => entity.SetField("Down", 0));

            entity.OnNotify("forwardReleased", (ent) => entity.SetField("Forward", 0));
            entity.OnNotify("backwardReleased", (ent) => entity.SetField("Backward", 0));
            entity.OnNotify("leftReleased", (ent) => entity.SetField("Left", 0));
            entity.OnNotify("rightReleased", (ent) => entity.SetField("Right", 0));

            entity.SetField("InitializedFly", 1);
        }

        internal static void DoFly(Entity entity)
        {
            BaseScript.OnInterval(0, delegate
            { 
                if (!entity.IsFieldTrue("EnableFly"))
                    return false;

                if (entity.IsOnGround())
                    return true;

                Vector3 Velocity = entity.GetVelocity();

                Velocity.Z = 23;

                if (entity.IsFieldTrue("Up"))
                    Velocity.Z += 300;

                if (entity.IsFieldTrue("Down"))
                    Velocity.Z -= 300;

                if (!entity.IsFieldTrue("Forward")
                    && !entity.IsFieldTrue("Backward")
                    && !entity.IsFieldTrue("Left")
                    && !entity.IsFieldTrue("Right"))
                {
                    if (Velocity.X > 0)
                        Velocity.X -= 1;
                    else if (Velocity.X < 0)
                        Velocity.X += 1;

                    if (Velocity.Y > 0)
                        Velocity.Y -= 1;
                    else
                        Velocity.Y += 1;
                }

                entity.SetVelocity(Velocity);
                return true;
            });
        }
    }
}

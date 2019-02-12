using Andromeda;
using Andromeda.Events;
using Andromeda.Events.EventArguments;
using InfinityScript;
using InfinityScript.PBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AntiCheat.ACModules
{
    //FIXME, Rewrite PLZ
    public class Aimbot : IAntiCheatModule
    {
        public string Name => "Anti-Aimbot";

        public string Description => "Checks if a player is using Aimbot";

        public bool Enabled
        {
            get;
            set;
        } = Config.Instance.AntiAimbot.Enabled;

        public Action<Entity, string> TakeAction
        {
            get;
            set;
        } = new Action<Entity, string>((ent, reason) =>
        {
            Common.Admin.Ban(ent, "AntiCheat", reason);
        });

        int MaxAngleChange = 70;
        int MaxAngleChangeTime = 150;

        int MaxTagHit = 5;
        int MaxTagHitTime = 10000;


        public void RegisterEvents()
        {
            Script.PlayerDamage.Add((sender, args) =>
            {
                Entity entity = sender as Entity;
                string tag = args.Hitloc;

                if (args.Mod != "MOD_BULLET" || !entity.IsPlayer)
                    return;

                long changeTime = GetChangeTimeAndRegisterNewKill(entity);
                long tagTime = GetTagTimeAndRegisterNewKill(entity);

                int change = GetChangeAndRegisterNewKill(entity);
                int tagHit = GetTagHitAndRegisterNewKill(entity, tag);

                if (changeTime < MaxAngleChangeTime && change > MaxAngleChange)
                {              
                    entity.Tell($"Hax? Time: {changeTime}. Change: {change}");
                }
                else if(tagTime > MaxTagHitTime)
                {
                    if (tagHit > MaxTagHit)
                        entity.Tell("Hax m9?");
                    else
                        ResetTagHitTime(entity);
                }
            });
        }

        #region Tags
        private int GetHighestTagHitCount(Entity entity)
        {
            if (entity.HasField("TagHits"))
                return entity.GetField<Dictionary<string, int>>("TagHits").OrderByDescending(x => x.Key).FirstOrDefault().Value;

            return 0;
        }

        private int GetTagHitAndRegisterNewKill(Entity entity, string tag)
        {
            int hits = GetHighestTagHitCount(entity);

            if (entity.HasField("TagHits"))
            {
                if (entity.GetField<Dictionary<string, int>>("TagHits").ContainsKey(tag))
                    entity.GetField<Dictionary<string, int>>("TagHits")[tag]++;
                else
                    entity.GetField<Dictionary<string, int>>("TagHits")[tag] = 1;
            }
            else
            {
                Dictionary<string, int> hit = new Dictionary<string, int>
                {
                    { tag, 1 }
                };

                entity.SetFieldT("TagHits", hit);
            }

            return hits;
        }

        private long GetTimeTags(Entity entity) => entity.GetFieldOrVal<Stopwatch>("TagStopwatch").ElapsedMilliseconds;

        private long GetTagTimeAndRegisterNewKill(Entity entity)
        {
            if (entity.HasField("TagStopwatch"))
            {
                long last = GetTimeTags(entity);

                return last;
            }
            else
            {
                entity.SetFieldT("TagStopwatch", new Stopwatch());

                entity.GetField<Stopwatch>("TagStopwatch").Start();

                return -1;
            }
        }

        private void ResetTagHitTime(Entity entity) => entity.GetField<Stopwatch>("TagStopwatch").Restart();
        #endregion

        #region Angle
        private int GetLongestVectorChange(Entity entity)
        {
            if (entity.HasField("LastKillAngle"))
            {          
                Vector3 lastKillAngle = entity.GetField<Vector3>("LastKillAngle");

                return (int)lastKillAngle.DistanceToAngle(entity.GetPlayerAngles());
                
            }

            return 0;
        }

        private int GetChangeAndRegisterNewKill(Entity entity)
        {
            int change = GetLongestVectorChange(entity);

            entity.SetField("LastKillAngle", entity.GetPlayerAngles());

            return change;
        }

        private long GetTimeSinceLastKill(Entity entity) => entity.GetFieldOrVal<Stopwatch>("AngleStopwatch").ElapsedMilliseconds;

        private long GetChangeTimeAndRegisterNewKill(Entity entity)
        {
            if (entity.HasField("AngleStopwatch"))
            {
                long last = GetTimeSinceLastKill(entity);

                entity.GetField<Stopwatch>("AngleStopwatch").Restart();

                return last;
            }
            else
            {
                entity.SetFieldT("AngleStopwatch", new Stopwatch());

                entity.GetField<Stopwatch>("AngleStopwatch").Start();

                return -1;
            }
        }
        #endregion
    }
}

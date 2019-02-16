using InfinityScript.Events;
using System;
using System.Linq;

namespace InfinityScript.PBase
{
    public class PluginBase : BaseScript
    {
        public PluginBase()
        {
            PlayerConnected += delegate (Entity player)
            {
                Script.PlayerConnected.Run(player, player);
            };

            PlayerDisconnected += delegate (Entity player)
            {
                Script.PlayerDisconnected.Run(player, player);
            };

            Tick += delegate ()
            {
                Script.Tick.Run(this);
            };

            Notified += delegate (int id, string str, Parameter[] param)
            {
                if (str == "trigger")
                    return;

                var data = new NotifyArgs(id, str, param);

                //Log.Info($"Notify: id: {id}, str: {str}, param: {string.Join<Parameter>(", ", param)}");

                if (id >= 0 && id < 18)
                    Script.PlayerNotified.Run(this, data);

                Script.Notified.Run(this, data);
            };

            Script.Instance = this;

            Script.Init();
        }

        public override void OnExitLevel()
        {
            Script.OnExitLevel.Run(this);
        }

        public override void OnPlayerDamage(Entity player, Entity inflictor, Entity attacker, int damage, int dFlags, string mod, string weapon, Vector3 point, Vector3 dir, string hitLoc)
        {
            int baseDamage = damage;
            PlayerDamageArgs data = new PlayerDamageArgs(player, inflictor, attacker, damage, dFlags, mod, weapon, point, dir, hitLoc);

            Script.PlayerDamage.Run(attacker, data);
            data.Update(out damage);

            if (damage > player.Health + baseDamage && damage > baseDamage)
                AfterDelay(1, () => player.FinishPlayerDamage(inflictor, attacker, damage, dFlags, mod, weapon, point, dir, hitLoc, 0f));
            else
                player.Health += baseDamage - damage;
        }

        public override void OnPlayerDisconnect(Entity player)
        {
            Script.PlayerDisconnecting.Run(player, player);
        }

        public override void OnPlayerKilled(Entity player, Entity inflictor, Entity attacker, int damage, string mod, string weapon, Vector3 dir, string hitLoc)
        {
            PlayerKilledArgs data = new PlayerKilledArgs(player, inflictor, attacker, damage, mod, weapon, dir, hitLoc);

            Script.PlayerKilled.Run(attacker, data);
        }

        public override EventEat OnSay3(Entity player, ChatType type, string name, ref string message)
        {
            var data = new PlayerSayArgs(player, type, name, message);

            Script.PlayerSay.Run(player, data);

            data.Update(out message);

            return data.Eaten ? EventEat.EatGame | EventEat.EatScript : EventEat.EatNone;
        }

        public override void OnStartGameType()
        {
            Script.OnStartGametype.Run(this);
        }

        public override void OnPlayerConnecting(Entity player)
        {
            Script.PlayerConnecting.Run(player, player);
        }

        public override void OnPlayerLastStand(Entity player, Entity inflictor, Entity attacker, int damage, string mod, string weapon, Vector3 dir, string hitLoc, int timeOffset, int deathAnimDuration)
        {
            int baseDamage = damage;
            PlayerLastStandArgs data = new PlayerLastStandArgs(player, inflictor, attacker, damage, mod, weapon, dir, hitLoc, timeOffset, deathAnimDuration);

            Script.PlayerLastStand.Run(attacker, data);

            data.Update(out damage);

            player.Health += baseDamage - damage;
        }

        public override string OnPlayerRequestConnection(string playerName, string playerHWID, string playerXUID, string playerIP, string playerSteamID, string playerXNAddress)
        {
            var data = new ConnectionRequestArgs(playerName, playerHWID, playerXUID, playerIP, playerSteamID, playerXNAddress);

            Script.ConnectionRequest.Run(this, data);

            if (data.Eaten)
                return data.DisconnectMessage;

            return null;
        }

        public override void OnVehicleDamage(Entity vehicle, Entity inflictor, Entity attacker, int damage, int dFlags, string mod, string weapon, Vector3 point, Vector3 dir, string hitLoc, int timeOffset, int modelIndex, string partName)
        {
            // won't bother too much for now
            base.OnVehicleDamage(vehicle, inflictor, attacker, damage, dFlags, mod, weapon, point, dir, hitLoc, timeOffset, modelIndex, partName);
        }
    }
}

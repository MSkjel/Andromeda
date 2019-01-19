using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfinityScript.Events
{
    public class PlayerLastStandArgs : EventArgs
    {
        public Entity Player { get; private set; }
        public Entity Inflictor { get; private set; }
        public Entity Attacker { get; private set; }
        public int Damage { get; set; }
        public string Mod { get; private set; }
        public string Weapon { get; private set; }
        public Vector3 Dir { get; private set; }
        public string HitLoc { get; private set; }
        public int TimeOffset { get; private set; }
        public int DeathAnimDuration { get; private set; }

        public PlayerLastStandArgs(Entity player, Entity inflictor, Entity attacker, int damage, string mod, string weapon, Vector3 dir, string hitLoc, int timeOffset, int deathAnimDuration)
        {
            Player = player;
            Inflictor = inflictor;
            Attacker = attacker;
            Damage = damage;
            Mod = mod;
            Weapon = weapon;
            Dir = dir;
            HitLoc = hitLoc;
            TimeOffset = timeOffset;
            DeathAnimDuration = deathAnimDuration;
        }

        public void Deconstruct(out Entity player, out Entity inflictor, out Entity attacker, out int damage, out string mod, out string weapon, out Vector3 dir, out string hitLoc, out int timeOffset, out int deathAnimDuration)
        {
            player = Player;
            inflictor = Inflictor;
            attacker = Attacker;
            damage = Damage;
            mod = Mod;
            weapon = Weapon;
            dir = Dir;
            hitLoc = HitLoc;
            timeOffset = TimeOffset;
            deathAnimDuration = DeathAnimDuration;
        }

        public void Update(out int damage)
        {
            damage = Damage;
        }
    }
}

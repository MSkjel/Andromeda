using InfinityScript;
using System;

namespace PBase.Events
{
    public class PlayerKilledArgs : EventArgs
    {
        public Entity Player { get; private set; }
        public Entity Inflictor { get; private set; }
        public Entity Attacker { get; private set; }

        public int Damage { get; private set; }

        public string Mod { get; private set; }
        public string Weapon { get; private set; }

        public string Hitloc { get; private set; }

        public Vector3 Dir { get; private set; }

        public PlayerKilledArgs(Entity player, Entity inflictor, Entity attacker, int damage, string mod, string weapon, Vector3 dir, string hitLoc)
        {
            Player = player;
            Inflictor = inflictor;
            Attacker = attacker;
            Damage = damage;
            Mod = mod;
            Weapon = weapon;
            Hitloc = hitLoc;
            Dir = dir;
        }

        public void Deconstruct(out Entity player, out Entity inflictor, out Entity attacker, out int damage, out string mod, out string weapon, out Vector3 dir, out string hitLoc)
        {
            player = Player;
            inflictor = Inflictor;
            attacker = Attacker;
            damage = Damage;
            mod = Mod;
            weapon = Weapon;
            dir = Dir;
            hitLoc = Hitloc;
        }
    }
}

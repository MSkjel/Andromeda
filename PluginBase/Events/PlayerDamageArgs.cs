using InfinityScript;
using System;

namespace PBase.Events
{
    public class PlayerDamageArgs : EventArgs
    {

        public Entity Player { get; private set; }
        public Entity Inflictor { get; private set; }
        public Entity Attacker { get; private set; }

        public int Damage { get; set; }
        public int DFlags { get; private set; }

        public string Mod { get; private set; }
        public string Weapon { get; private set; }

        public string Hitloc { get; private set; }

        public Vector3 Point { get; private set; }
        public Vector3 Dir { get; private set; }

        public PlayerDamageArgs(Entity player, Entity inflictor, Entity attacker, int damage, int dFlags, string mod, string weapon, Vector3 point, Vector3 dir, string hitLoc)
        {
            Player = player;
            Inflictor = inflictor;
            Attacker = attacker;
            Damage = damage;
            DFlags = dFlags;
            Mod = mod;
            Weapon = weapon;
            Hitloc = hitLoc;
            Point = point;
            Dir = dir;
        }

        public void Deconstruct(out Entity player, out Entity inflictor, out Entity attacker, out int damage, out int dFlags, out string mod, out string weapon, out Vector3 point, out Vector3 dir, out string hitLoc)
        {
            player = Player;
            inflictor = Inflictor;
            attacker = Attacker;
            damage = Damage;
            dFlags = DFlags;
            mod = Mod;
            weapon = Weapon;
            point = Point;
            dir = Dir;
            hitLoc = Hitloc;
        }

        public void Update(out int damage)
        {
            damage = Damage;
        }
    }
}

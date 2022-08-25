//using Andromeda.Events;
//using InfinityScript;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text;

//namespace AntiCheat.ACModules
//{
//    internal class AntiEntity : IAntiCheatModule
//    {
//        public string Name => "Anti-Entity";

//        //public string Description => "Checks if a player is using Entity(Cheat)";

//        public string AdminPermission => "anticheat.warn.entity";

//        public bool Enabled
//        {
//            get;
//            set;
//        } = Config.Instance.AntiEntity.Enabled;


//        public void RegisterEvents()
//        {
//            Script.PlayerConnected.Add((_, ent) =>
//            {
//                ent.NotifyOnPlayerCommand("Used_Melee", "+melee_zoom");
//                ent.NotifyOnPlayerCommand("Used_Melee", "+melee");
//                ent.NotifyOnPlayerCommand("Used_F", "+activate");
//                ent.NotifyOnPlayerCommand("Used_R", "+reload");

//                ent.OnNotify("Used_F", (pl) =>
//                {
//                    if (pl.HasField("Last_Entity_Menu_Open") && pl.GetField<Stopwatch>("Last_Entity_Menu_Open").ElapsedMilliseconds < 5000)
//                    {
//                        Utils.WarnAdminsWithPerm(pl, AdminPermission, $"%eYou might want to take a look at %p{pl.Name}%e. Entity(Cheat) suspected. Pressed F");
//                    }
//                });

//                ent.OnNotify("Used_R", (pl) =>
//                {
//                    if (pl.HasField("Last_Entity_Menu_Open") && pl.GetField<Stopwatch>("Last_Entity_Menu_Open").ElapsedMilliseconds < 5000)
//                    {
//                        Utils.WarnAdminsWithPerm(pl, AdminPermission, $"%eYou might want to take a look at %p{pl.Name}%e. Entity(Cheat) suspected. Pressed R");
//                    }
//                });

//                ent.OnNotify("Used_Melee", (pl) =>
//                {
//                    if (pl.AdsButtonPressed() && pl.GetVelocity().X == 0 && pl.GetVelocity().Y == 0)
//                    {

//                        BaseScript.AfterDelay(800, () =>
//                        {
//                            if (!pl.HasField("Last_Melee") || pl.GetField<Stopwatch>("Last_Melee").ElapsedMilliseconds > 1500)
//                            {
//                                if (!pl.HasField("Last_Entity_Menu_Open"))
//                                {
//                                    Stopwatch watch = new Stopwatch();
//                                    watch.Start();
//                                    pl.SetField("Last_Entity_Menu_Open", new Parameter(watch));
//                                }
//                                else
//                                    pl.GetField<Stopwatch>("Last_Entity_Menu_Open").Restart();

//                                Utils.WarnAdminsWithPerm(pl, AdminPermission, $"%eYou might want to take a look at %p{pl.Name}%e. Entity(Cheat) suspected. Opened menu");
//                            }
//                        });
//                    }
//                });
//            });

//            Script.PlayerDamage.Add((_, arg) =>
//            {
//                if (arg.Mod == "MOD_MELEE")
//                {
//                    if (!arg.Attacker.HasField("Last_Melee"))
//                    {
//                        Stopwatch watch = new Stopwatch();
//                        watch.Start();
//                        arg.Attacker.SetField("Last_Melee", new Parameter(watch));
//                    }
//                    else
//                        arg.Attacker.GetField<Stopwatch>("Last_Melee").Restart();
//                }
//            });

//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.IO;
using Andromeda;
using InfinityScript;
using static InfinityScript.BaseScript;

namespace BaseAdmin
{
    public class Utils
    {
        public static readonly List<GameMap> Maps = new List<GameMap>()
        {
            #region Stock
            new GameMap("mp_seatown", "Seatown"),
            new GameMap("mp_mogadishu", "Bakaara"),
            new GameMap("mp_dome", "Dome"),
            new GameMap("mp_paris", "Resistance"),
            new GameMap("mp_village", "Village"),
            new GameMap("mp_bootleg", "Bootleg"),
            new GameMap("mp_carbon", "Carbon"),
            new GameMap("mp_interchange", "Interchange"),
            new GameMap("mp_hardhat", "Hardhat"),
            new GameMap("mp_exchange", "Downturn"),
            new GameMap("mp_radar", "Outpost"),
            new GameMap("mp_lambeth", "Fallen"),
            new GameMap("mp_alpha", "Lockdown"),
            new GameMap("mp_bravo", "Mission"),
            new GameMap("mp_underground", "Underground"),
            new GameMap("mp_plaza2", "Arkaden"),
            #endregion

            #region DLC1
            new GameMap("mp_overwatch", "Overwatch"),
            new GameMap("mp_park", "Liberation"),
            new GameMap("mp_italy", "Piazza"),
            new GameMap("mp_morningwood", "Black Box", "BlackBox"),
            #endregion

            #region DLC2
            new GameMap("mp_cement", "Foundation"),
            new GameMap("mp_meteora", "Sanctuary"),
            new GameMap("mp_qadeem", "Oasis"),
            #endregion

            #region DLC4
            new GameMap("mp_shipbreaker", "Decomission"),
            new GameMap("mp_nola", "Parish"),
            new GameMap("mp_roughneck", "Off Shore"),
            new GameMap("mp_boardwalk", "Boardwalk"),
            new GameMap("mp_moab", "Gulch"),
            #endregion

            #region Faceoff and Terminal
            new GameMap("mp_hillside_ss", "Getaway"),
            new GameMap("mp_restrepo_ss", "Lookout"),
            new GameMap("mp_aground_ss", "Aground"),
            new GameMap("mp_burn_ss", "U-Turn", "UTurn"),
            new GameMap("mp_courtyard_ss", "Erosion"),
            new GameMap("mp_six_ss", "Vortex"),
            new GameMap("mp_crosswalk_ss", "Intersection"),
            new GameMap("mp_terminal_cls", "Terminal"),
            #endregion       
        };

        public static bool MapFilesExist(string MapCode)
        {
            foreach (string dir in Directory.GetDirectories(Directory.GetCurrentDirectory() + @"\zone"))
                foreach (string map in Directory.GetFiles(dir, "*.ff"))
                    if (map.ToLower().Contains(MapCode.ToLower() + ".ff") && Path.GetFileName(map).ToLower().StartsWith("mp_"))
                        return true;

            return false;
        }

        public static void CountPlayers(out int axis, out int allies, out int none, out int spectators)
        {
            axis = 0; allies = 0; none = 0; spectators = 0;

            foreach (var client in Players)
            {
                switch (client.SessionTeam)
                {
                    case "axis":
                        axis++;
                        break;

                    case "allies":
                        allies++;
                        break;

                    case "spectator":
                        spectators++;
                        break;

                    case "none":
                        none++;
                        break;

                    default:
                        //shit's wrong, mate
                        break;
                }
            }
        }

        public static bool IsTrue(string option)
        {
            switch (option.ToLowerInvariant())
            {
                case "on":
                case "1":
                case "true":
                    return true;

                case "off":
                case "0":
                case "false":
                default:
                    return false;
            }
        }

        public static bool CurrentGamemodeIsTeamBased()
        {
            CountPlayers(out int axis, out int allies, out int none, out int spectators);

            return axis == 0 && allies == 0 && none > 0;
        }

        public static bool CaseInsensitiveContains(string str1, string str2)
            => str1.IndexOf(str2, StringComparison.InvariantCultureIgnoreCase) != -1;

        public static bool CaseInsensitiveStartsWith(string str1, string str2)
            => str1.StartsWith(str2, StringComparison.InvariantCultureIgnoreCase);
    }
}

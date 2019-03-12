using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Andromeda;
using Andromeda.Events;
using InfinityScript;

namespace AntiCheat.ACModules
{
    public class ForceClass : IAntiCheatModule
    {
        public string Name => "Anti-Forceclass";

        public string Description => "Checks if a player is using Forceclass";

        public bool Enabled
        {
            get;
            set;
        } = Config.Instance.AntiForceClass.Enabled;

        public Action<Entity, string> TakeAction
        {
            get;
            set;
        } = new Action<Entity, string>((ent, reason) =>
        {
            Common.Admin.Ban(ent, "AntiCheat", reason);
        });


        private List<string> BlockedWeapons = new List<string>();
        private List<string> BlockedAttachments = new List<string>();
        private List<string> BlockedPerks = new List<string>();
        private List<string> BlockedKillstreaks = new List<string>();

        public void RegisterEvents()
        {

            Events.DSRLoad.Add((sender, args) =>
            {
                string[] dsr = File.ReadAllLines($"{Directory.GetCurrentDirectory()}\\admin\\{args.DSR}.dsr");

                foreach (string line in dsr)
                {
                    if (line.Contains("commonOption.weaponRestricted"))
                    {
                        string[] split2 = line.Split(new[] { "commonOption.weaponRestricted." }, StringSplitOptions.None)[1].Split('"');

                        if (split2[1].Contains("1"))
                            BlockedWeapons.Add(split2[0].Trim() + "_mp");
                    }
                    else if (line.Contains("commonOption.killstreakRestricted"))
                    {
                        string[] split2 = line.Split(new[] { "commonOption.killstreakRestricted." }, StringSplitOptions.None)[1].Split('"');

                        if (split2[1].Contains("1"))
                            BlockedKillstreaks.Add(split2[0].Trim());
                    }
                    else if (line.Contains("commonOption.perkRestricted"))
                    {
                        string[] split2 = line.Split(new[] { "commonOption.perkRestricted." }, StringSplitOptions.None)[1].Split('"');

                        if (split2[1].Contains("1"))
                            BlockedPerks.Add(split2[0].Trim());
                    }
                    else if (line.Contains("commonOption.attachmentRestricted"))
                    {
                        string[] split2 = line.Split(new[] { "commonOption.attachmentRestricted." }, StringSplitOptions.None)[1].Split('"');

                        if (split2[1].Contains("1"))
                            BlockedAttachments.Add(split2[0].Trim());
                    }
                }
            });

            Events.WeaponFired.Add((sender, args) =>
            {
                if (!args.Player.HasField("Allow_Weapon_Name") || args.Player.CurrentWeapon != args.Player.GetField<string>("Allow_Weapon_Name"))
                {
                    if (CheckIfBadWeapon(args.Weapon))
                        TakeAction(args.Player, $"^1Force-Class detected. Weapon: ^7{args.Weapon}");
                    else if (CheckIfBadAttachment(args.Weapon, out string attachment))
                        TakeAction(args.Player, $"^1Force-Class detected. Attachment: ^7{attachment}");
                }
            });

            Events.GrenadePullback.Add((sender, args) =>
            {
                if (!args.Player.HasField("Allow_Grenade_Name") || args.Player.CurrentWeapon != args.Player.GetField<string>("Allow_Grenade_Name"))
                {
                    if (CheckIfBadGrenade(args.Grenade))
                        TakeAction(args.Player, $"^1Force-Class detected. Grenade: ^7{args.Grenade}");
                }
            });

            Events.EarnedKillstreak.Add((sender, args) =>
            {
                if (CheckIfBadKillStreak(args.Killstreak))
                {
                    args.Player.IncrementField("Bad_Killstreak", 1);

                    if(args.Player.IsFieldHigherOrEqual("Bad_Killstreak", 2))
                        TakeAction(args.Player, $"^1Force-Class detected. Killstreak: ^7{args.Killstreak}");
                    else
                        Utils.WarnAdminsWithPerm(args.Player, "anticheat.warn.forceclass.killstreak", $"%eYou might want to take a look at %p{args.Player.Name}%e. Bad killstreak detected: %h1{args.Killstreak}");
                }
            });
        }

        private bool CheckIfBadWeapon(string weapon)
        {
            return BlockedWeapons.Contains(weapon.Substring(0, weapon.GetNthIndexWeapon('_', 3)));
        }

        private bool CheckIfBadGrenade(string grenade) => BlockedPerks.Contains(grenade);

        private bool CheckIfBadAttachment(string weapon, out string badattachment)
        {
            IEnumerable<string> attachments = weapon.Split('_').ToList().Skip(3);
            badattachment = "";

            foreach (string attachment in attachments)
                if (BlockedAttachments.Contains(attachment))
                {
                    badattachment = attachment;

                    return true;
                }

            return false;
        }

        private bool CheckIfBadKillStreak(string killstreak) => !string.IsNullOrEmpty(killstreak) && BlockedKillstreaks.Contains(killstreak);
    }
}

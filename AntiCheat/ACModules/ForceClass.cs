//#define LowMemory

#if !LowMemory

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
    internal class ForceClass : IAntiCheatModule
    {
        public string Name => "Anti-Forceclass";

        public string Description => "Checks if a player is using Forceclass";

        public bool Enabled
        {
            get;
            set;
        } = Config.Instance.AntiForceClass.Enabled;

        public Lazy<Action<Entity, string>> TakeAction
        {
            get;
            set;
        } = new Lazy<Action<Entity, string>>(delegate
        {
            return new Action<Entity, string>((ent, reason) =>
            {
                Common.Admin.Ban(ent, "AntiCheat", reason);
            });
        });


        private List<string> BlockedWeapons = new List<string>();
        private List<string> BlockedAttachments = new List<string>();
        private List<string> BlockedPerks = new List<string>();
        //private List<string> BlockedKillstreaks = new List<string>();

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
                    //else if (line.Contains("commonOption.killstreakRestricted"))
                    //{
                    //    string[] split2 = line.Split(new[] { "commonOption.killstreakRestricted." }, StringSplitOptions.None)[1].Split('"');

                    //    if (split2[1].Contains("1"))
                    //        BlockedKillstreaks.Add(split2[0].Trim());
                    //}
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

            Events.ChangedClass.Add((sender, args) =>
            {
                Entity ent = args.Player;

                if (ent.RequestPermission("anticheat.immune.forceclass", out _))
                    return;

                if (!ent.HasField("Allow_Jugg") && (args.ClassName == "axis" || args.ClassName == "allies"))
                    Common.Admin.Ban(ent, "AntiCheat", $"^1Force-Class detected. Juggernaut");
                else if(args.ClassName.Contains("custom"))
                {
                    int index = int.Parse(args.ClassName.Replace("custom", "")) - 1;

                    string primary = Utilities.BuildWeaponName(
                        ent.GetPlayerData("customClasses", index, "weaponSetups", 0, "weapon").ToString(),
                        ent.GetPlayerData("customClasses", index, "weaponSetups", 0, "attachment", 0).ToString(),
                        ent.GetPlayerData("customClasses", index, "weaponSetups", 0, "attachment", 1).ToString(),
                        0,
                        0);

                    string secondary = Utilities.BuildWeaponName(
                        ent.GetPlayerData("customClasses", index, "weaponSetups", 1, "weapon").ToString(),
                        ent.GetPlayerData("customClasses", index, "weaponSetups", 1, "attachment", 0).ToString(),
                        ent.GetPlayerData("customClasses", index, "weaponSetups", 1, "attachment", 1).ToString(),
                        0,
                        0);

                    string grenade = ent.GetPlayerData("customClasses", index, "perks", 0).ToString();
                    //string specialGrenade = ent.GetPlayerData("customClasses", index, "specialGrenade").ToString();

                    if (CheckIfBadWeapon(primary))
                        Common.Admin.Ban(ent, "AntiCheat", $"Force-Class detected. Weapon: {primary}");
                    else if (CheckIfBadAttachment(primary, out string attachment))
                        Common.Admin.Ban(ent, "AntiCheat", $"Force-Class detected. Attachment: {attachment}");
                    else if (CheckIfBadWeapon(secondary))
                        Common.Admin.Ban(ent, "AntiCheat", $"Force-Class detected. Weapon: {secondary}");
                    else if (CheckIfBadAttachment(secondary, out string secAttachment))
                        Common.Admin.Ban(ent, "AntiCheat", $"Force-Class detected. Attachment: {secAttachment}");
                    else if (CheckIfBadGrenade(grenade))
                        Common.Admin.Ban(args.Player, "AntiCheat", $"Force-Class detected. Grenade: {grenade}");
                }
            });

            //Events.EarnedKillstreak.Add((sender, args) =>
            //{
            //    if (args.Player.RequestPermission("anticheat.immune.forceclass", out _))
            //        return;

            //    if (CheckIfBadKillStreak(args.Killstreak))
            //    {
            //        args.Player.IncrementField("Bad_Killstreak", 1);

            //        if (args.Player.IsFieldHigherOrEqual("Bad_Killstreak", 2))
            //            Common.Admin.Ban(args.Player, "AntiCheat", $"^1Force-Class detected. Killstreak: ^7{args.Killstreak}");
            //        else
            //            Utils.WarnAdminsWithPerm(args.Player, "anticheat.warn.forceclass.killstreak", $"%eYou might want to take a look at %p{args.Player.Name}%e. Bad killstreak detected: %h1{args.Killstreak}");
            //    }
            //});
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

        //private bool CheckIfBadKillStreak(string killstreak) => !string.IsNullOrEmpty(killstreak) && BlockedKillstreaks.Contains(killstreak);
    }
}
#endif
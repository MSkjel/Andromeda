using Andromeda;
using Andromeda.Events;
using InfinityScript;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace AntiCheat.ACModules
{
    internal class AntiProxy : IAntiCheatModule
    {
        public string Name => "Anti-Proxy";

        public string Description => "Checks if a player is using Proxy or VPN";

        public string AdminPermission => "anticheat.warn.proxy";

        public bool Enabled
        {
            get;
            set;
        } = Config.Instance.AntiProxy.Enabled;

        public Action<Entity, string> TakeAction
        {
            get;
            set;
        } = new Action<Entity, string>((ent, reason) =>
        {

        });

        static readonly CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

        public void RegisterEvents()
        {
            if(Enabled)
                Events.PreMatchDone.Add((_, args) =>
                {
                    Script.PlayerConnected.Add((__, ent) =>
                    {
                        if (Utils.OnlineAdminsWithPerms(AdminPermission).Count() > 0)
                        {
                            Uri uri;

                            if(!Uri.TryCreate($"http://ipqualityscore.com/api/json/ip/KUO1M4XABodNQfJmDOIWRIIf2U6nBUyO/{ent.IP.Address}?strictness=1&allow_public_access_points=true", UriKind.Absolute, out uri))
                                return;

                            using (WebClient client = new WebClient())
                            {
                                client.DownloadStringAsync(uri);

                                client.DownloadStringCompleted += (sender, resp) =>
                                {
                                    var responseJson = JsonConvert.DeserializeObject<Dictionary<string, string>>(resp.Result);

                                    if (responseJson["success"] == "true")
                                    {
                                        float score = float.Parse(responseJson["fraud_score"]) / 100f;

                                        if (score > Config.Instance.AntiProxy.Threshold)
                                        {
                                            //string country = new RegionInfo(cultures.Where(x => x.TwoLetterISOLanguageName.ToLower() == responseJson["country_code"].ToLower()).FirstOrDefault().LCID).DisplayName;

                                            string[] messages =
                                            {
                                                $"%p{ent.Name}'s %e IP has a very low trust score",
                                                $"%eScore: %h1{score:0.00}/{Config.Instance.AntiProxy.Threshold:0.00}",
                                                $"%eCountry: %h1{/*country ?? "Unknown"*/ responseJson["country_code"]}",
                                                $"%eCity: %h1{responseJson["city"]}"
                                            };

                                            Utils.WarnAdminsWithPerm(ent, AdminPermission, messages);
                                        }
                                    }
                                };
                            }
                        }
                    });
                });
        }
    }
}

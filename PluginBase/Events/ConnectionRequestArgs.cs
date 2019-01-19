using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfinityScript.Events
{
    public class ConnectionRequestArgs : EatableEventArgs
    {
        public string Name { get; private set; }
        public string HWID { get; private set; }
        public string XUID { get; private set; }
        public string IP { get; private set; }
        public string SteamID { get; private set; }
        public string XNAddress { get; private set; }

        public string DisconnectMessage { get; set; }

        public ConnectionRequestArgs(string name, string hwid, string xuid, string ip, string steamid, string xnaddr)
        {
            Name = name;
            HWID = hwid;
            XUID = xuid;
            IP = ip;
            SteamID = steamid;
            XNAddress = xnaddr;

            DisconnectMessage = null;
        }

        public void Reject(string message)
        {
            DisconnectMessage = message;
            Eat();
        }
    }
}

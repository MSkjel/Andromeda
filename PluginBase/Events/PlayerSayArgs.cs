using InfinityScript;
using static InfinityScript.BaseScript;
using System;

namespace PBase.Events
{
    public class PlayerSayArgs : EatableEventArgs
    {
        public Entity Player { get; private set; }
        public ChatType ChatType { get; private set; }
        public string Name { get; private set; }
        public string Message { get; set; }

        public PlayerSayArgs(Entity player, ChatType chatType, string name, string message)
        {
            Player = player;
            ChatType = chatType;
            Name = name;
            Message = message;
        }

        public void Update(out string message)
        {
            message = Message;
        }
    }
}

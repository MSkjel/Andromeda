using InfinityScript.Events;
using System;
using System.Collections;
using System.Collections.Generic;

namespace InfinityScript
{
    public static partial class Script
    {
        public static readonly InternalEvent Tick = new InternalEvent(ErrorHandler(nameof(Tick)));
        public static readonly InternalEvent OnExitLevel = new InternalEvent(ErrorHandler(nameof(OnExitLevel)));
        public static readonly InternalEvent OnStartGametype = new InternalEvent(ErrorHandler(nameof(OnStartGametype)));
        public static readonly InternalEvent<NotifyArgs> OtherNotified = new InternalEvent<NotifyArgs>(ErrorHandler(nameof(OtherNotified)));
        public static readonly InternalEvent<NotifyArgs> PlayerNotified = new InternalEvent<NotifyArgs>(ErrorHandler(nameof(PlayerNotified)));
        public static readonly InternalEvent<Entity> PlayerConnected = new InternalEvent<Entity>(ErrorHandler(nameof(PlayerConnected)));
        public static readonly InternalEvent<Entity> PlayerConnecting = new InternalEvent<Entity>(ErrorHandler(nameof(PlayerConnecting)));
        public static readonly InternalEvent<Entity> PlayerDisconnected = new InternalEvent<Entity>(ErrorHandler(nameof(PlayerDisconnected)));
        public static readonly InternalEvent<Entity> PlayerDisconnecting = new InternalEvent<Entity>(ErrorHandler(nameof(PlayerDisconnecting)));
        public static readonly InternalEvent<PlayerDamageArgs> PlayerDamage = new InternalEvent<PlayerDamageArgs>(ErrorHandler(nameof(PlayerDamage)));
        public static readonly InternalEvent<PlayerKilledArgs> PlayerKilled = new InternalEvent<PlayerKilledArgs>(ErrorHandler(nameof(PlayerKilled)));
        public static readonly InternalEvent<PlayerSayArgs> PlayerSay = new InternalEvent<PlayerSayArgs>(ErrorHandler(nameof(PlayerSay)));
        public static readonly InternalEvent<PlayerLastStandArgs> PlayerLastStand = new InternalEvent<PlayerLastStandArgs>(ErrorHandler(nameof(PlayerLastStand)));
        public static readonly InternalEvent<ConnectionRequestArgs> ConnectionRequest = new InternalEvent<ConnectionRequestArgs>(ErrorHandler(nameof(ConnectionRequest)));

        public static event Action<string> WaitTillEvents
        {
            add => Instance.WaitTillEvents += value;
            remove => Instance.WaitTillEvents -= value;
        }

        public static IEnumerator WaitTill(string type)
            => Instance.WaitTill(type);

        public static IEnumerator WaitTillAny(params string[] types)
            => Instance.WaitTill_any(types);

        public static IEnumerator WaitTillAnyOrTimeout(string type, int timeout, Action<string> action = null)
            => Instance.WaitTill_notify_or_timeout(type, timeout, action);
    }
}

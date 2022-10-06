using Networking;
using Unity.Collections;
using Unity.Netcode;

namespace Events
{
    public struct ConnectionEventMessage : INetworkSerializeByMemcpy
    {
        public ConnectStatus ConnectStatus;
        public FixedString32Bytes PlayerName;
    }
}
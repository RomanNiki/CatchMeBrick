using System;

namespace Networking
{
    [Serializable]
    public class ConnectionPayload
    {
        public string ClientGUID;
        public int ClientScene = -1;
        public string PlayerName;
        public bool IsDebug;
    }
}
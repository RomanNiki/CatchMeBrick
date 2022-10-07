using Unity.Netcode;

namespace Loading
{
    public class NetworkedLoadingProgressTracker : NetworkBehaviour
    {
        public NetworkVariable<float> Progress { get; } = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    }
}
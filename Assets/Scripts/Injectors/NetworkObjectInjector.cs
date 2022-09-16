using Unity.Netcode;

namespace Injectors
{
    public class NetworkObjectInjector : NetworkBehaviour
    {
        public override void OnNetworkSpawn()
        {
            InjectClientRpc();
        }

        [ClientRpc]
        private void InjectClientRpc()
        {
            var factory = FindObjectOfType<Injector>();
            if (factory)
            {
                factory.InjectNetworkObject(this);
            }
        }
    }
}
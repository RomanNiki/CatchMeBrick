using Unity.Netcode;
using Zenject;

namespace Injectors
{
    public class Injector : NetworkBehaviour
    {
        [Inject] private DiContainer _container;
        
        public void InjectNetworkObject(NetworkObjectInjector networkObjectInjector)
        {
            if (networkObjectInjector)
            {
                var o = networkObjectInjector.gameObject;
                _container.InjectGameObject(o);
            }
        }
    }
}
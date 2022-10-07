using Damageable;
using Unity.Netcode;
using Zenject;

namespace Player
{
    public class PlayerView : NetworkBehaviour, IDamageable
    {
        private PlayerModel _model;

        [Inject]
        public void Constructor(PlayerModel model)
        {
            _model = model;
        }

        [ServerRpc]
        public void TakeDamageServerRpc(float damage)
        {
            _model.TakeDamage(damage);
        }
    }
}
using Followers.Camera;
using Unity.Netcode;
using Zenject;

namespace Damageable.Player
{
    public class CameraController : NetworkBehaviour
    {
        [Inject] private CameraFollower _cam;

        private void Start()
        {
            SetCameraClientRpc();
        }

        [ClientRpc]
        private void SetCameraClientRpc()
        {
            if (_cam == null || !IsOwner)
            {
                return;
            }
            _cam.SetTarget(transform);
        }
    }
}
using Followers.Camera;
using Unity.Netcode;

namespace Damageable.Player
{
    public class CameraController : NetworkBehaviour
    {
        private CameraFollower _cam;

        private void Start()
        {
            SetCameraClientRpc();
        }

        [ClientRpc]
        private void SetCameraClientRpc()
        {
            _cam = FindObjectOfType<CameraFollower>();
            if (_cam == null || !IsOwner)
            {
                return;
            }
            _cam.SetTarget(transform);
        }
    }
}
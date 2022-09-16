using UniRx;
using Unity.Netcode;
using UnityEngine;

namespace Damageable.Player
{
    public class PlayerTransform : NetworkBehaviour
    {
        private readonly CompositeDisposable _disposable = new();
        [SerializeField] private bool _serverAuth;
        [SerializeField] private float _cheapInterpolationTime = 0.1f;

        private NetworkVariable<PlayerNetworkState> _playerState;
        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            var permission = _serverAuth ? NetworkVariableWritePermission.Server : NetworkVariableWritePermission.Owner;
            _playerState = new NetworkVariable<PlayerNetworkState>(writePerm: permission);
        }

        private void OnEnable()
        {
            Observable.EveryFixedUpdate().Subscribe(_ =>
            {
                if (IsOwner) TransmitState();
                else ConsumeState();
            }).AddTo(_disposable);
        }

        private void OnDisable()
        {
            _disposable.Clear();
        }

        #region Transmit State

        private void TransmitState()
        {
            var state = new PlayerNetworkState
            {
                Position = _rigidbody.position,
                Rotation = transform.rotation.eulerAngles
            };

            if (IsServer || !_serverAuth)
                _playerState.Value = state;
            else
                TransmitStateServerRpc(state);
        }

        [ServerRpc]
        private void TransmitStateServerRpc(PlayerNetworkState state)
        {
            _playerState.Value = state;
        }

        #endregion

        #region Interpolate State

        private Vector3 _posVel;
        private float _rotVelY;

        private void ConsumeState()
        {
            _rigidbody.MovePosition(Vector3.SmoothDamp(_rigidbody.position, _playerState.Value.Position, ref _posVel,
                _cheapInterpolationTime));
            transform.rotation = Quaternion.Euler(
                0,
                Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, _playerState.Value.Rotation.y, ref _rotVelY,
                    _cheapInterpolationTime), 0);
        }

        #endregion

        private struct PlayerNetworkState : INetworkSerializable
        {
            private float _posX, _posY, _posZ;
            private short _rotY;

            internal Vector3 Position
            {
                get => new(_posX, _posY, _posZ);
                set
                {
                    _posX = value.x;
                    _posY = value.y;
                    _posZ = value.z;
                }
            }

            internal Vector3 Rotation
            {
                get => new(0, _rotY, 0);
                set => _rotY = (short) value.y;
            }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref _posX);
                serializer.SerializeValue(ref _posY);
                serializer.SerializeValue(ref _posZ);

                serializer.SerializeValue(ref _rotY);
            }
        }
    }
}
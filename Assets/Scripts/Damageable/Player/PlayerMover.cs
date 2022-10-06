using Inputs.InputTypes;
using UniRx;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Damageable.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMover : NetworkBehaviour
    {
        private readonly CompositeDisposable _disposable = new();
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _rotationSpeed;
        private IInput _input;
        private Rigidbody _rigidbody;
        private Camera _camera;

        [Inject]
        public void Constructor(IInput input, Camera cam)
        {
            _input = input;
            _input.Enable();
            _camera = cam;
            Observable.EveryLateUpdate().Subscribe(_ =>
            {
                FixedTick();
            }).AddTo(_disposable);
        }
        
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnDisable()
        {
            _input?.Disable();
            _disposable.Clear();
        }
        
        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                OnDisable();
                Destroy(this);
            }
        }

        private void FixedTick()
        {
            if (!IsOwner || _input == null) return;
            var direction = _input.MoveDirection;
            var camDirection = _camera.transform.rotation * direction;
            var moveDirection = new Vector3(camDirection.x, 0, camDirection.z);
            Move(moveDirection * (_moveSpeed * Time.fixedDeltaTime));
            Rotate(moveDirection);
        }

        private void Move(Vector3 moveDirection)
        {
            _rigidbody.MovePosition(transform.position + moveDirection); 
        }

        private void Rotate(Vector3 moveDirection)
        {
            moveDirection.y = 0;
            if (Vector3.Angle(transform.forward, moveDirection) <= 0) return;
            var newDirection =
                Vector3.RotateTowards(transform.forward, moveDirection, _rotationSpeed * Time.fixedDeltaTime, 0);
            transform.rotation = Quaternion.LookRotation(newDirection);
        }
    }
}
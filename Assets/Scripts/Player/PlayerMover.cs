using System;
using Inputs.InputTypes;
using UnityEngine;
using Zenject;

namespace Player
{
    public class PlayerMover : IFixedTickable, IDisposable, IInitializable
    {
        private readonly IInput _input;
        private readonly Camera _camera;
        private readonly PlayerModel _model;
        private readonly Settings _settings;
        
        public PlayerMover(IInput input, Camera cam, PlayerModel model, Settings settings)
        {
            _input = input;
            _camera = cam;
            _model = model;
            _settings = settings;
        }

        public void Initialize()
        {
            _input.Enable();
        }

        public void Dispose()
        {
            _input?.Disable();
        }

        public void FixedTick()
        {
            var direction = _input.MoveDirection;
            var camDirection = _camera.transform.rotation * direction;
            var moveDirection = new Vector3(camDirection.x, 0, camDirection.z);
            _model.Move(moveDirection * (_settings.MoveSpeed * Time.fixedDeltaTime));
            Rotate(moveDirection);
        }

        private void Rotate(Vector3 moveDirection)
        {
            moveDirection.y = 0;
            if (Vector3.Angle(_model.Forward, moveDirection) <= 0) return;
            var newDirection =
                Vector3.RotateTowards(_model.Forward, moveDirection, _settings.RotationSpeed * Time.fixedDeltaTime, 0);
            _model.Rotation = Quaternion.LookRotation(newDirection);
        }
        
        [Serializable]
        public class Settings
        {
            public float RotationSpeed;
            public float MoveSpeed;
        }
    }
}
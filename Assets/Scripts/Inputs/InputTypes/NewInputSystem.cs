using UnityEngine;
using Zenject;

namespace Inputs.InputTypes
{
    public class NewInputSystem : Input
    {
        private readonly PlayerInput _input;
        
        [Inject]
        public NewInputSystem(PlayerInput input)
        {
            _input = input;
        }

        public override void Enable()
        {
            base.Enable();
            _input.Enable();
        }

        public override void Disable()
        {
            base.Disable();
            _input.Disable();
        }
        
        protected override Vector2 GetMoveDirection()
        {
            return _input.Player.Move.ReadValue<Vector2>();
        }
    }
}
using UnityEngine;
using Zenject;

namespace Inputs.InputTypes
{
    public class NewInputSystem : Input
    {
        private PlayerInput _input;
        
        [Inject]
        public void Constructor(PlayerInput input)
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
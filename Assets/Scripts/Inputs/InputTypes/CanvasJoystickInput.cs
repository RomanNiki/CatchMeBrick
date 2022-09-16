using UnityEngine;
using Zenject;

namespace Inputs.InputTypes
{
    public class CanvasJoystickInput : Input
    {
        private Joystick _joystick;

        [Inject]
        public void Constructor(Joystick joystick)
        {
            _joystick = joystick;
        }

        protected override Vector2 GetMoveDirection()
        {
            return _joystick.Direction;
        }
    }
}
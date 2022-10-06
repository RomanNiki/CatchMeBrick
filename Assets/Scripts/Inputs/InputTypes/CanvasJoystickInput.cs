using UnityEngine;
using Zenject;

namespace Inputs.InputTypes
{
    public class CanvasJoystickInput : Input
    {
        private readonly Joystick _joystick;

        [Inject]
        public CanvasJoystickInput(Joystick joystick)
        {
            _joystick = joystick;
        }

        protected override Vector2 GetMoveDirection()
        {
            return _joystick.Direction;
        }
    }
}
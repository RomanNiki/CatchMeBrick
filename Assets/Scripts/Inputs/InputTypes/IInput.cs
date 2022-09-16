using UnityEngine;

namespace Inputs.InputTypes
{
    public interface IInput
    {
        Vector3 MoveDirection { get; }
        void Enable();
        void Disable();
    }
}
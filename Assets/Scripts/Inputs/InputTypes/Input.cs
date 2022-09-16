using UniRx;
using UnityEngine;

namespace Inputs.InputTypes
{
    public abstract class Input: IInput
    {
        private readonly CompositeDisposable _disposable = new ();
        public Vector3 MoveDirection { get; private set; }
        public virtual void Enable()
        {
            Observable.EveryUpdate().Subscribe(_ => OnMove(GetMoveDirection())).AddTo(_disposable);
        }
        
        public virtual void Disable()
        {
            _disposable.Clear();
        }
        
        protected abstract Vector2 GetMoveDirection();
        
        private void OnMove(Vector2 moveDirection)
        {
            MoveDirection = new Vector3(moveDirection.x, 0f, moveDirection.y);
        }
    }
}
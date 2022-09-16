using UniRx;
using UnityEngine;

namespace Followers.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraFollower : Follower
    {
        private readonly CompositeDisposable _followTargetDisposable = new();
        private readonly CompositeDisposable _lookAtDisposable = new();
        private Transform _lookAtTarget;

        public override void SetTarget(Transform target)
        {
            base.SetTarget(target);
            _offset.z *= FollowTarget.position.z < 0 ? 1 : -1;
            transform.rotation = Quaternion.LookRotation(FollowTarget.position - (FollowTarget.position + _offset));
            Observable.EveryFixedUpdate().Subscribe(_  =>
            {
                if (FollowTarget != null)
                {
                    Move(Time.deltaTime);
                }
                else
                {
                    _followTargetDisposable.Clear();
                }
               
            }).AddTo(_followTargetDisposable);
        }

        public void LookAt(Transform target)
        {
            _lookAtTarget = target;
            Observable.EveryFixedUpdate().Subscribe(_  =>
            {
                if (_lookAtTarget != null)
                {
                    transform.rotation = Quaternion.LookRotation(FollowTarget.position - (transform.position));
                }
                else
                {
                    _lookAtDisposable.Clear();
                }
            }).AddTo(_lookAtDisposable);
        }

        private void OnDisable()
        {
            _followTargetDisposable.Clear();
            _lookAtDisposable.Clear();
        }
    }
}
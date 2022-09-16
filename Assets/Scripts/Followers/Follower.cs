using UnityEngine;

namespace Followers
{
    public abstract class Follower : MonoBehaviour, IFollower
    {
        protected Transform FollowTarget;
        [SerializeField] protected Vector3 _offset;
        [SerializeField] private float _smoothing = 1f;
        
        public virtual void SetTarget(Transform target)
        {
            FollowTarget = target;
            transform.position = FollowTarget.position + _offset;
        }

        protected void Move(float deltaTime)
        {
            var nextPosition = Vector3.Lerp(transform.position, FollowTarget.position + _offset, deltaTime * _smoothing);
            transform.position = nextPosition;
        }
    }
}
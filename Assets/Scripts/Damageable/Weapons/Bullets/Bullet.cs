using UnityEngine;

namespace Damageable.Weapons.Bullets
{
    public class Bullet : MonoBehaviour, IBullet
    {
        [SerializeField] private float _damage;
        
        public void Hit(IDamageable damageable)
        {
            damageable.TakeDamageServerRpc(_damage);
        }

        private void OnCollisionEnter(Collision collision)
        {
            var damageable = collision.collider.GetComponent<IDamageable>();
            damageable?.TakeDamageServerRpc(_damage);
        }
    }
}
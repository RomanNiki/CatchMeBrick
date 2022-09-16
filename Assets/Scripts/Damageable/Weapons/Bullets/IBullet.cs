namespace Damageable.Weapons.Bullets
{
    public interface IBullet
    {
        void Hit(IDamageable damageable);
    }
}
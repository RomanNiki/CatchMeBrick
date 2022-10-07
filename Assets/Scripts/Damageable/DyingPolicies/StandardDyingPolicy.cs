namespace Damageable.DyingPolicies
{
    public class StandardDyingPolicy : IDyingPolicy
    {
        public bool Died(float value)
        {
            return value <= 0;
        }
    }
}
namespace Damageables.DyingPolicies
{
    public class StandartDyingPolicy : IDyingPolicy
    {
        public bool Died(float value)
        {
            return value <= 0;
        }
    }
}
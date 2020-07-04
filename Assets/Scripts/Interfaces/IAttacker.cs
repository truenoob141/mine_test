namespace Mine.Game
{
    public interface IAttacker : IEntity
    {
        float GetDamage();
        void DealDamage(float damage);
    }
}

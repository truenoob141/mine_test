namespace Mine.Game
{
    public interface IVictim : IEntity
    {
        bool IsAlive();

        /// <returns>Final damage amount</returns>
        float TakeDamage(float amount);
    }
}

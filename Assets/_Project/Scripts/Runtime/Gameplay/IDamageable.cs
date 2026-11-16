namespace Clover.Gameplay
{
    /// <summary>Anything a weapon can strike.</summary>
    public interface IDamageable
    {
        void TakeHit(in Hit hit);
    }
}

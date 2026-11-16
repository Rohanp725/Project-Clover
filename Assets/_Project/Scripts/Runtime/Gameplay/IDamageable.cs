namespace Clover.Gameplay
{
    /// <summary>Anything a weapon can strike.</summary>
    public interface IDamageable
    {
        HitOutcome TakeHit(in Hit hit);
    }
}

namespace Clover.Gameplay
{
    /// <summary>Something an element's affliction can be laid on.</summary>
    public interface IAfflictable
    {
        AfflictionSet Afflictions { get; }
    }
}

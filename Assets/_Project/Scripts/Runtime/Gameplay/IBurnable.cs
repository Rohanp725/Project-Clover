namespace Clover.Gameplay
{
    /// <summary>Something that can be set burning, and asked whether it is.</summary>
    public interface IBurnable
    {
        BurnStatus Burn { get; }
    }
}

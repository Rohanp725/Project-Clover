namespace Clover.Gameplay
{
    /// <summary>Something that can be knocked open, and so can be finished.</summary>
    public interface IStaggerable
    {
        bool IsStaggered { get; }
    }
}

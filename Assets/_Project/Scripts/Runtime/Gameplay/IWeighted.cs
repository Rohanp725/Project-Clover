namespace Clover.Gameplay
{
    /// <summary>Something with a weight a chain can feel: 1 is a soldier. What is too heavy to be dragged pulls the one who drags instead.</summary>
    public interface IWeighted
    {
        float Weight { get; }
    }
}

namespace Clover.UI
{
    /// <summary>One thing the pause menu can do. <see cref="None"/> is what a press gives when it only asked "are you sure".</summary>
    public enum PauseEntry
    {
        None,
        Resume,
        Controls,
        GiveUp,
        Forget,
        Quit,
    }
}

namespace Clover.UI
{
    /// <summary>One thing the pause menu can do. <see cref="None"/> is what a press gives when it only asked "are you sure".</summary>
    public enum PauseEntry
    {
        None,
        Resume,
        Controls,
        Fullscreen,
        GiveUp,
        Forget,
        Quit,
        /// <summary>The playtest's guide to every effect.</summary>
        Guide,
        /// <summary>The playtest's version and what each update added.</summary>
        WhatsNew,
        /// <summary>The playtest's way home from any level.</summary>
        ProvingGround,
    }
}

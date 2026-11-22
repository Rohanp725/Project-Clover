namespace Clover.UI
{
    /// <summary>
    /// The short line under the Shards count that says Memories were just paid (way_build_plan.md question 10, the
    /// owner's answer): "+4 Memories" for two seconds. Payments that land while it shows add to it and keep it up for two
    /// seconds more. Plain, so its rules are tested without a HUD; <see cref="MemoryGain"/> draws it.
    /// </summary>
    public sealed class MemoryLine
    {
        /// <summary>Seconds the line stays up after the last payment.</summary>
        public const float Shown = 2f;
        /// <summary>Seconds at the end over which it fades.</summary>
        public const float Fade = 0.3f;

        /// <summary>The Memories the line says.</summary>
        public int Amount { get; private set; }
        /// <summary>Seconds left to show it.</summary>
        public float Left { get; private set; }
        /// <summary>Goes up whenever the words change, so the text is set only then.</summary>
        public int Version { get; private set; }

        public bool Visible => Left > 0f;

        /// <summary>How opaque the line is now: full, then fading over its last moment.</summary>
        public float Alpha => Left <= 0f ? 0f : Left >= Fade ? 1f : Left / Fade;

        /// <summary>A payment has landed. Nothing for none.</summary>
        public void Add(int memories)
        {
            if (memories <= 0) return;
            if (!Visible) Amount = 0;
            Amount += memories;
            Left = Shown;
            Version++;
        }

        /// <summary>Time passes.</summary>
        public void Tick(float deltaTime)
        {
            if (Left <= 0f) return;
            Left -= deltaTime;
            if (Left > 0f) return;
            Left = 0f;
            Amount = 0;
            Version++;
        }

        /// <summary>The words for an amount: "+1 Memory", "+4 Memories".</summary>
        public static string Words(int amount) => amount == 1 ? "+1 Memory" : "+" + amount + " Memories";
    }
}

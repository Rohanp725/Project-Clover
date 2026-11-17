using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// The plain rules of Falling Sun, the Fire god's gift: the sun's road across a fight. At dawn it is rolled along
    /// the ground to the enemy faced, at noon it is carried up and hangs, at dusk it sets on what is under it; a blow
    /// taken sends it under, to rise again if the surge has time left.
    /// </summary>
    public static class SunRules
    {
        /// <summary>Metres a second the sun is rolled at dawn, how far at most, and for how long at most.</summary>
        public const float DawnSpeed = 6f, DawnReach = 9f, DawnSeconds = 2.5f;
        /// <summary>How near the enemy faced the rolled sun must come for dawn to be over.</summary>
        public const float DawnTouch = 0.9f;
        /// <summary>Metres across: the sun at dawn, and grown at noon.</summary>
        public const float DawnSize = 1f, NoonSize = 1.35f;
        /// <summary>What the rolled sun does to the first enemy it meets.</summary>
        public const float DawnDamage = 14f, DawnStagger = 20f;
        public const int DawnStacks = 2;
        /// <summary>The burning trail behind the rolled sun: half its width, how long it burns, its bite and how often it bites.</summary>
        public const float TrailHalfWidth = 0.8f, TrailSeconds = 4f, TrailDamage = 4f, TrailEvery = 0.5f;
        /// <summary>Metres over the enemy faced at which the sun hangs at noon, and metres from the player within which burns deepen each second.</summary>
        public const float Height = 3.8f, BurnReach = 6f;
        /// <summary>Seconds the sun takes to set.</summary>
        public const float FallSeconds = 0.32f;
        /// <summary>Called down while still being rolled, it bursts at this share of its weight, this many metres wide.</summary>
        public const float CalledShare = 0.5f, CalledRadius = 2.5f;
        /// <summary>Sent under by a blow, it rises again after this long, if the surge had this long left when it went.</summary>
        public const float UnderSeconds = 1.5f, RiseAgainNeeds = 3f;

        /// <summary>True when dawn is over: the sun has reached the enemy faced, been rolled as far as it goes, or its time is up.</summary>
        public static bool DawnOver(float seconds, float rolled, float gap) => seconds >= DawnSeconds || rolled >= DawnReach || gap <= DawnTouch;

        /// <summary>How much heavier the setting sun lands for the burn stacks it finds under it: 8% each, up to double.</summary>
        public static float Weight(int burnStacks) => 1f + Mathf.Min(1f, 0.08f * Mathf.Max(0, burnStacks));

        /// <summary>True when a sun sent under by a blow comes back: the surge must have time enough left for a new dawn.</summary>
        public static bool RisesAgain(float surgeSecondsLeft) => surgeSecondsLeft >= RiseAgainNeeds;
    }
}

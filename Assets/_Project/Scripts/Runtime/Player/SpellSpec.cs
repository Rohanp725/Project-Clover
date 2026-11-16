namespace Clover.Player
{
    /// <summary>
    /// The numbers of a spell, as its form reads them. Each form says which of these it uses; the rest are ignored.
    /// Distances are in metres, times in seconds.
    /// </summary>
    public class SpellSpec
    {
        /// <summary>Mana it costs.</summary>
        public float Cost = 30f;
        public float Damage = 20f;
        public float Stagger = 20f;
        /// <summary>How far it reaches from the player.</summary>
        public float Reach = 6f;
        /// <summary>The radius of whatever it strikes with: a burst, a crater, a patch.</summary>
        public float Radius = 2.5f;
        /// <summary>Full width of a cone, in degrees.</summary>
        public float Arc = 70f;
        /// <summary>How long it or what it leaves lasts.</summary>
        public float Seconds = 4f;
        /// <summary>Seconds between bites of what it leaves.</summary>
        public float Interval = 0.5f;
        /// <summary>How many: leaps of a bolt, blades, bullets.</summary>
        public int Count = 2;
        /// <summary>Stacks of the element's affliction it lays on what it strikes.</summary>
        public int Stacks = 1;
        /// <summary>Seconds that affliction lasts; zero for its usual time.</summary>
        public float AfflictionSeconds;
        /// <summary>Seconds before it lands or fires.</summary>
        public float Delay;
        public float Speed = 10f;
        /// <summary>How high a leap rises.</summary>
        public float Height = 3.4f;
        /// <summary>Damage each bite of what it leaves behind deals.</summary>
        public float LingerDamage = 5f;
        /// <summary>Health the cast takes and leaves grey, to be won back by blows. A spell with this may cost no mana. It cannot be cast by one who has no more health than this.</summary>
        public float HealthCost;
    }
}

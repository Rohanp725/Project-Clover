using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// The plain rules of Sunspot, Fire's brand: light blows feed it, up to a limit, and one heavy blow sets it off in
    /// a single burst that is bigger for every blow fed. Left alone until its time runs out, it bursts at half.
    /// </summary>
    public static class SunspotRules
    {
        /// <summary>What each blow fed adds to the burst, for a brand whose own burst is 12 damage and 10 stagger.</summary>
        public const float DamageAFeed = 9f, StaggerAFeed = 8f;
        /// <summary>The share of the burst it gives when nobody sets it off.</summary>
        public const float LeftAlone = 0.5f;

        /// <summary>The blows fed after one more light blow lands: never more than the brand holds.</summary>
        public static int Feed(int fed, int most) => Mathf.Min(Mathf.Max(0, most), Mathf.Max(0, fed) + 1);

        /// <summary>The burst's damage: its own, and more for each blow fed. <paramref name="scale"/> is what the player's marks add to spells.</summary>
        public static float Damage(float own, int fed, float scale = 1f) => own + DamageAFeed * Mathf.Max(0, fed) * scale;

        public static float Stagger(float own, int fed) => own + StaggerAFeed * Mathf.Max(0, fed);
    }
}

using System;

namespace Clover.Player.Magic
{
    /// <summary>
    /// What an infused strike asks before it starts (STEEL_AND_ELEMENT.md 3.4; steel/README.md "The infused strike"):
    /// each strike button has its own clock (L2 with R1 on <see cref="SpellCooldowns.StrikeFirst"/>, L2 with R2 on
    /// <see cref="SpellCooldowns.StrikeSecond"/>), 8 s, cut by weapon play like a spell's; the deep strike (all in on one
    /// element, which is every Hand in Phase 1) also costs 20 mana. Plain C#: the caller pays and starts the clock only
    /// when this says yes.
    /// </summary>
    [Serializable]
    public class StrikeRules
    {
        public float Seconds = 8f;
        public float DeepMana = 20f;

        /// <summary>
        /// Whether a strike is the deep strike: L2 with R2 unless two different elements are carried. All in, R2 is the
        /// element's deep strike (Burnt Offering, Down the Channel ...), given under its Devotion; with one element carried
        /// it is too (IMPLEMENTATION.md 8.5, question 3), so a Hand with one element has both strikes. In a mix, R2 is the
        /// second element's infused strike.
        /// </summary>
        public static bool IsDeep(bool second, bool mixed) => second && !mixed;

        /// <summary>The clock of a strike button: the first (R1) or the second (R2).</summary>
        public static int Clock(bool second) => second ? SpellCooldowns.StrikeSecond : SpellCooldowns.StrikeFirst;

        /// <summary>Whether the strike may start now, and if not, why.</summary>
        public StrikeRefusal Check(SpellCooldowns clocks, bool second, bool deep, float mana, bool hasElement)
        {
            if (!hasElement) return StrikeRefusal.NoElement;
            if (clocks != null && !clocks.Ready(Clock(second))) return StrikeRefusal.NotReady;
            if (deep && mana < DeepMana) return StrikeRefusal.NoMana;
            return StrikeRefusal.None;
        }

        /// <summary>What the strike costs in mana: the deep strike's price, or nothing.</summary>
        public float Price(bool deep) => deep ? DeepMana : 0f;

        /// <summary>Starts the button's clock; <paramref name="cooldownRate"/> is how fast the player's clocks run (1 as written).</summary>
        public void Begin(SpellCooldowns clocks, bool second, float cooldownRate = 1f) =>
            clocks?.Start(Clock(second), Seconds / Math.Max(0.1f, cooldownRate));
    }

    public enum StrikeRefusal
    {
        None,
        NoElement,
        NotReady,
        NoMana,
    }
}

using System;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// Things that happen to enemies that the player's upgrades answer: one dies, an affliction on one reaches full
    /// depth, or one that is Soaked carries what was laid on it to those near. The enemy only says so here; it does
    /// not know who is listening. Listeners must not strike in answer at once (the blow that caused this is still
    /// being counted): they note it and act on their own next step.
    /// </summary>
    public static class CombatEvents
    {
        public static event Action<Component, AfflictionSet> Died;
        public static event Action<Component, AfflictionKind> AfflictionFull;
        public static event Action<Component, AfflictionKind, int> Carried;
        /// <summary>An affliction was laid on something (how many stacks were asked, and what laid it): the moment marks answer a laying (the ledger's laying-answer).</summary>
        public static event Action<Component, AfflictionKind, int, LaySource> Laid;
        /// <summary>A frozen thing was shattered by a heavy blow.</summary>
        public static event Action<Component> Shattered;
        /// <summary>Something that was thrown into the air has come down.</summary>
        public static event Action<Component> Landed;

        // Domain reload is off: listeners from the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Died = null;
            AfflictionFull = null;
            Carried = null;
            Laid = null;
            LaidBy = LaySource.Effect;
            Shattered = null;
            Landed = null;
            PoisonBit = null;
            Reacted = null;
            Condition = null;
        }

        /// <summary>
        /// Two afflictions reacted on something: who, the pair (numbered as in <see cref="AfflictionPairs"/>), and the
        /// kind whose laying set it off (the partner is the pair's other kind). Raised by <see cref="ReactionEngine"/>.
        /// </summary>
        public static event Action<Component, int, AfflictionKind> Reacted;

        public static void RaiseReacted(Component who, int pair, AfflictionKind laid) => Reacted?.Invoke(who, pair, laid);

        /// <summary>
        /// An enemy condition began, beat, was struck or ended (<see cref="EnemyConditions"/>): on whom, and what. The
        /// listener runs the answers of the mark that set it, on its own step.
        /// </summary>
        public static event Action<Component, ConditionNews> Condition;

        public static void RaiseCondition(Component who, ConditionNews news) => Condition?.Invoke(who, news);

        /// <summary>Poison bit something on its beat (or was made to): who, how deep it was, and what the bite dealt.</summary>
        public static event Action<Component, int, float> PoisonBit;

        public static void RaisePoisonBit(Component who, int depth, float damage) => PoisonBit?.Invoke(who, depth, damage);

        public static void RaiseLanded(Component who) => Landed?.Invoke(who);

        public static void RaiseShattered(Component who) => Shattered?.Invoke(who);

        public static void RaiseDied(Component who, AfflictionSet afflictions) => Died?.Invoke(who, afflictions);

        /// <summary>What laid the affliction an <see cref="AfflictionFull"/> or <see cref="Carried"/> listener is hearing of, while it hears.</summary>
        public static LaySource LaidBy { get; private set; }

        /// <summary>Tells the listeners what came of laying an affliction on something, and what laid it.</summary>
        public static void Report(Component who, AfflictionKind kind, int stacks, in AfflictionResult result, LaySource source = LaySource.Effect)
        {
            LaidBy = source;
            if (result.ReachedFull) AfflictionFull?.Invoke(who, kind);
            if (result.Carried) Carried?.Invoke(who, kind, stacks);
            if (result.Applied && stacks > 0) Laid?.Invoke(who, kind, stacks, source);
            LaidBy = LaySource.Effect;
        }
    }
}

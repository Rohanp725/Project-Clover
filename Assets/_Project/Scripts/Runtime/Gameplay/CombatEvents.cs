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

        // Domain reload is off: listeners from the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Died = null;
            AfflictionFull = null;
            Carried = null;
        }

        public static void RaiseDied(Component who, AfflictionSet afflictions) => Died?.Invoke(who, afflictions);

        /// <summary>Tells the listeners what came of laying an affliction on something.</summary>
        public static void Report(Component who, AfflictionKind kind, int stacks, in AfflictionResult result)
        {
            if (result.ReachedFull) AfflictionFull?.Invoke(who, kind);
            if (result.Carried) Carried?.Invoke(who, kind, stacks);
        }
    }
}

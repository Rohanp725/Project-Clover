using System.Collections.Generic;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The action <see cref="UpgradeAction.Condition"/>: sets an enemy condition on the trigger's enemy, or on those near
    /// it, and lays what the effect names on each that takes it (IMPLEMENTATION.md 3.7). What resists being held resists
    /// a condition that holds or slows the same way: an elite takes half the slowing, and what cannot be frozen (a god's
    /// form) takes no such condition at all. Kept out of <see cref="PlayerUpgrades"/>, which only calls it.
    /// </summary>
    public static class ConditionCaster
    {
        /// <summary>Sets it; returns on how many it was set.</summary>
        public static int Cast(UpgradeEffect effect, Component subject, Vector3 centre, PlayerCombat combat, LaySource source, HashSet<IDamageable> scratch)
        {
            var spec = effect != null ? effect.State : null;
            if (spec == null || string.IsNullOrEmpty(spec.Id) || combat == null) return 0;
            AnswersOf(effect, spec.Id, out bool struck, out bool ended, out bool died);
            int mark = effect.Owner != null ? effect.Owner.Index : -1;
            if (!spec.ToOthers && effect.Radius <= 0f) return Set(effect, spec, subject, null, mark, struck, ended, died, combat, source) ? 1 : 0;

            // Round the trigger's enemy: the nearest first, as many as the effect allows (all within the radius when it names none).
            scratch.Clear();
            if (spec.ToOthers && subject is IDamageable self) scratch.Add(self);
            int most = effect.Count > 0 ? effect.Count : 12;
            float radius = effect.Radius > 0f ? effect.Radius : 5f;
            int set = 0;
            while (set < most && scratch.Count < 24)
            {
                var near = combat.EnemyNear(centre, radius, scratch);
                if (!(near is IDamageable one)) break;
                scratch.Add(one);
                // Only those carrying what the effect seeks (Chokedamp's damp runs to the shaken alone).
                if (effect.LeapsTo != AfflictionKind.None && !(near is IAfflictable seeker && seeker.Afflictions.Has(effect.LeapsTo))) continue;
                if (Set(effect, spec, near, spec.ToOthers ? subject : null, mark, struck, ended, died, combat, source)) set++;
            }
            scratch.Clear();
            return set;
        }

        private static bool Set(UpgradeEffect effect, ConditionSpec spec, Component who, Component link, int mark, bool struck, bool ended, bool died, PlayerCombat combat, LaySource source)
        {
            if (!(who is IAfflictable afflictable)) return false;
            var set = afflictable.Afflictions;
            bool holds = (spec.Flags & ConditionFlags.Rooted) != 0 || spec.Pace < 1f;
            if (holds && !set.CanFreeze) return false;
            float slowShare = set.FreezeScale < 1f ? 0.5f : 1f;
            if (set.Conditions.Set(spec, who, mark, link, struck, ended, slowShare, died) < 0) return false;
            if (effect.Affliction != AfflictionKind.None && effect.Stacks > 0)
                CombatEvents.Report(who, effect.Affliction, effect.Stacks, set.Lay(effect.Affliction, effect.Stacks + combat.ExtraStacks, 0f, combat.AfflictionPower, source, who));
            return true;
        }

        // Whether the mark has anything to do when a blow lands on the condition, when it ends, or when the enemy dies with
        // it: only then is it told.
        private static void AnswersOf(UpgradeEffect effect, string id, out bool struck, out bool ended, out bool died)
        {
            struck = false;
            ended = false;
            died = false;
            if (effect.Owner == null) return;
            foreach (var answer in effect.Owner.Effects)
            {
                if (answer.Condition != id) continue;
                if (answer.On == UpgradeTrigger.ConditionStruck) struck = true;
                if (answer.On == UpgradeTrigger.ConditionEnded) ended = true;
                if (answer.On == UpgradeTrigger.ConditionDied) died = true;
            }
        }
    }
}

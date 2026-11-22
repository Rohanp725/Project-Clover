using System.Collections.Generic;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The action <see cref="UpgradeAction.Condition"/>: sets an enemy condition on the trigger's enemy, or on those near
    /// it, and lays what the effect names on each that takes it (IMPLEMENTATION.md 3.7). What resists being held resists
    /// a condition that holds, knocks flat or slows the same way: an elite takes half the slowing (and lies or is rooted
    /// half the time), what cannot be frozen (a god's form) takes no such condition at all, and no god is knocked flat,
    /// whatever its body. Kept out of
    /// <see cref="PlayerUpgrades"/>, which only calls it.
    /// </summary>
    public static class ConditionCaster
    {
        /// <summary>
        /// Sets it; returns on how many it was set. <paramref name="reactionPair"/> is the reaction whose own effect this is
        /// (-1: none): one that went off on the trigger's enemy as if laid sets its conditions there to last without their partner.
        /// </summary>
        public static int Cast(UpgradeEffect effect, Component subject, Vector3 centre, PlayerCombat combat, LaySource source, HashSet<IDamageable> scratch, int stacks = 1,
            float scale = -1f, float spread = 1f, Vector3? facing = null, int reactionPair = -1)
        {
            var spec = effect != null ? effect.State : null;
            if (spec == null || string.IsNullOrEmpty(spec.Id) || combat == null) return 0;
            int startAt = 0;
            if (reactionPair >= 0 && subject is IAfflictable reacted)
            {
                if (reacted.Afflictions.Unbound(reactionPair)) spec = Unbound(spec);
                startAt = spec.Blows > 0 ? reacted.Afflictions.CountStartsAt(reactionPair) : 0;
            }
            // One blow for each stack counted, its Blows at most (Iron Thorns): the rest counted as already drawn.
            if (spec.BlowsByStacks) startAt = Mathf.Max(startAt, BlowsDrawn(spec, stacks));
            float pace = PaceFor(spec, stacks);
            // Turning slowed by the stacks counted (Seized Joints): its run's one number is the turn it keeps.
            if (spec.TurnPerStack > 0f) scale = TurnFor(spec, stacks);
            AnswersOf(effect, spec.Id, out bool struck, out bool ended, out bool died, out bool attacked);
            int mark = effect.Owner != null ? effect.Owner.Index : -1;
            Vector3 toward = facing ?? centre;
            if (!spec.ToOthers && effect.Radius <= 0f) return Set(effect, spec, subject, null, mark, struck, ended, died, attacked, combat, source, pace, scale, toward, startAt) ? 1 : 0;

            // Round the trigger's enemy: the nearest first, as many as the effect allows (all within the radius when it names none).
            scratch.Clear();
            if (spec.ToOthers && subject is IDamageable self) scratch.Add(self);
            int most = effect.Count > 0 ? effect.Count : 12;
            float radius = (effect.Radius > 0f ? effect.Radius : 5f) * Mathf.Max(0.1f, spread);
            int set = 0;
            while (set < most && scratch.Count < 24)
            {
                var near = combat.EnemyNear(centre, radius, scratch);
                if (!(near is IDamageable one)) break;
                scratch.Add(one);
                // Only those carrying what the effect seeks (Chokedamp's damp runs to the shaken alone).
                if (effect.LeapsTo != AfflictionKind.None && !(near is IAfflictable seeker && seeker.Afflictions.Has(effect.LeapsTo))) continue;
                if (Set(effect, spec, near, spec.ToOthers ? subject : null, mark, struck, ended, died, attacked, combat, source, pace, scale, toward)) set++;
            }
            scratch.Clear();
            // No one near to chain it to: it is chained to the floor where it stands (Black Chain's 2 m).
            if (set == 0 && spec.ToOthers && spec.AloneLeash > 0f)
            {
                var alone = spec.Copy();
                alone.ToOthers = false;
                alone.Leash = spec.AloneLeash;
                alone.LeashToPlayer = false;
                alone.Flags &= ~ConditionFlags.Chained;
                if (Set(effect, alone, subject, null, mark, struck, ended, died, attacked, combat, source, pace, scale, toward)) set++;
            }
            return set;
        }

        /// <summary>
        /// The condition as it is set by a reaction that went off as if laid (The Sun Goes Under): the same, but lasting without
        /// the afflictions it would last with. The spec itself when it lasts with none.
        /// </summary>
        public static ConditionSpec Unbound(ConditionSpec spec)
        {
            if (spec == null || (spec.While == AfflictionKind.None && spec.While2 == AfflictionKind.None)) return spec;
            var loose = spec.Copy();
            loose.While = AfflictionKind.None;
            loose.While2 = AfflictionKind.None;
            return loose;
        }

        /// <summary>
        /// The pace a condition is set at for <paramref name="stacks"/> counted: by <see cref="ConditionSpec.SlowPerStack"/>
        /// when it has one (Cairn: 15% a stone), never below 0.05; -1 (the spec's own pace) otherwise.
        /// </summary>
        public static float TurnFor(ConditionSpec spec, int stacks) =>
            spec != null && spec.TurnPerStack > 0f ? Mathf.Max(0.1f, 1f - spec.TurnPerStack * Mathf.Max(1, stacks)) : 1f;

        /// <summary>The blows counted as drawn at the start for a condition set one blow a stack (<see cref="ConditionSpec.BlowsByStacks"/>).</summary>
        public static int BlowsDrawn(ConditionSpec spec, int stacks) =>
            spec != null && spec.BlowsByStacks && spec.Blows > 0 ? spec.Blows - Mathf.Clamp(stacks, 1, spec.Blows) : 0;

        public static float PaceFor(ConditionSpec spec, int stacks) =>
            spec != null && spec.SlowPerStack > 0f ? Mathf.Max(0.05f, 1f - spec.SlowPerStack * Mathf.Max(1, stacks)) : -1f;

        private static bool Set(UpgradeEffect effect, ConditionSpec spec, Component who, Component link, int mark, bool struck, bool ended, bool died, bool attacked,
            PlayerCombat combat, LaySource source, float pace, float scale, Vector3 toward, int startAt = 0)
        {
            if (!(who is IAfflictable afflictable)) return false;
            var set = afflictable.Afflictions;
            if (!Takes(spec, set)) return false;
            int slot = set.Conditions.Set(spec, who, mark, link, struck, ended, ShareOf(set), died, attacked, pace);
            if (slot < 0) return false;
            // A count begun along (the fig at two coils): the blows already drawn.
            if (startAt > 0) set.Conditions.StartAt(slot, startAt);
            // The trigger's own number kept as the run's (Collected Twice: the damage the reaction dealt).
            if (scale >= 0f) set.Conditions.Keep(slot, scale);
            // Turned to face a point: the spell's middle, or straight away from the player.
            if ((spec.Flags & ConditionFlags.FacesPoint) != 0)
                set.Conditions.FaceToward(slot, FacePoint(spec, who.transform.position, combat.transform.position, toward));
            if (effect.Affliction != AfflictionKind.None && effect.Stacks > 0)
                CombatEvents.Report(who, effect.Affliction, effect.Stacks, set.Lay(effect.Affliction, effect.Stacks + combat.ExtraStacks, 0f, combat.AfflictionPower, source, who), source);
            return true;
        }

        /// <summary>
        /// The point a condition that turns its carrier turns it to face: straight away from the player, 10 m on, when it faces
        /// away (Turned About); the spell's middle otherwise (Turned to the Sun's herb).
        /// </summary>
        public static Vector3 FacePoint(ConditionSpec spec, Vector3 carrier, Vector3 player, Vector3 spellMiddle)
        {
            if (spec == null || !spec.FaceAway) return spellMiddle;
            Vector3 away = carrier - player;
            away.y = 0f;
            return carrier + (away.sqrMagnitude > 0.0001f ? away.normalized : Vector3.forward) * 10f;
        }

        /// <summary>False when the enemy cannot take this condition: one that roots, knocks flat or slows, on what cannot be frozen; one that knocks flat, on a god.</summary>
        public static bool Takes(ConditionSpec spec, AfflictionSet set)
        {
            if (spec == null || set == null) return false;
            // A god only staggers (Capsize: "the great only stagger"); one in a body ice can hold is still never thrown down.
            if ((spec.Flags & ConditionFlags.Down) != 0 && set.Rank == EnemyRank.God) return false;
            // Nor is a god swung round to face a point.
            if ((spec.Flags & ConditionFlags.FacesPoint) != 0 && set.Rank == EnemyRank.God) return false;
            bool holds = (spec.Flags & (ConditionFlags.Rooted | ConditionFlags.Down)) != 0 || spec.Pace < 1f;
            return !holds || set.CanFreeze;
        }

        /// <summary>How much of a slowing, a root's time and a fall's time takes on this enemy: half on an elite, all on the rest.</summary>
        public static float ShareOf(AfflictionSet set) => set != null && set.FreezeScale < 1f ? 0.5f : 1f;

        // Whether the mark has anything to do when a blow lands on the condition, when it ends, or when the enemy dies with
        // it: only then is it told. A reaction's Shape mark answers from its shapes (task 4.5); each shape's ids are its own.
        private static void AnswersOf(UpgradeEffect effect, string id, out bool struck, out bool ended, out bool died, out bool attacked)
        {
            struck = false;
            ended = false;
            died = false;
            attacked = false;
            if (effect.Owner == null) return;
            Note(effect.Owner.Effects, id, ref struck, ref ended, ref died, ref attacked);
            if (effect.Owner.Shapes == null) return;
            foreach (var shape in effect.Owner.Shapes)
                if (shape != null) Note(shape.Effects, id, ref struck, ref ended, ref died, ref attacked);
        }

        private static void Note(UpgradeEffect[] answers, string id, ref bool struck, ref bool ended, ref bool died, ref bool attacked)
        {
            if (answers == null) return;
            foreach (var answer in answers)
            {
                if (answer.Condition != id) continue;
                if (answer.On == UpgradeTrigger.ConditionStruck) struck = true;
                if (answer.On == UpgradeTrigger.ConditionEnded) ended = true;
                if (answer.On == UpgradeTrigger.ConditionDied) died = true;
                if (answer.On == UpgradeTrigger.ConditionAttacked) attacked = true;
            }
        }
    }
}

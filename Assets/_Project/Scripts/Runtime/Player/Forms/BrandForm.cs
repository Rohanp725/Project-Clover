using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// A sign set on the enemy faced: it turns on the ground under them and hangs over their head for a while, and
    /// each of the next few blows of the weapon that land on them sets off a burst. With no enemy faced, nothing is
    /// branded and the mana is lost.
    /// </summary>
    public class BrandForm : RunningForm
    {
        public override string Id => SpellForms.Brand;

        public override int Cast(in SpellCast cast)
        {
            // A shape that sets the brand aside: only the hand's gesture, and the shape's own effects do the rest.
            if (cast.Spec.Quiet)
            {
                SpellWorks.Gesture(cast, FormKit.Cast, 0.3f);
                return 0;
            }
            var victim = FormKit.Victim(cast);
            if (victim == null)
            {
                ElementLook.Puff(cast.Element, cast.Origin + Vector3.up * 1.2f + cast.Forward, cast.Forward, 0.6f);
                if (cast.Defence != null) cast.Defence.Callout("No one to brand");
                return 0;
            }
            // A seal is slapped on from arm's length: the caster steps up to the enemy first (Paper Seal's 3 m step).
            if (cast.Spec.StepReach > 0f && cast.Player != null)
            {
                Vector3 to = victim.transform.position - cast.Origin;
                to.y = 0f;
                float gap = to.magnitude - ArmsLength;
                if (gap > 0.2f && to.sqrMagnitude > 0.01f)
                {
                    ElementLook.Puff(cast.Element, cast.Origin + Vector3.up * 0.4f, to.normalized, 0.6f);
                    cast.Player.StepTo(cast.Origin + to.normalized * Mathf.Min(gap, cast.Spec.StepReach));
                }
            }
            // One brand at a time.
            SpellWorks.EndRunsOf(this);
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.Victim = victim;
            run.Count = Mathf.Max(1, cast.Spec.Count);
            var marks = SpellWorks.Marks(run);
            marks[0] = FormFx.HoldMark(FormMark.Sigil, FormKit.Lit(cast.Colour, 2.4f));
            marks[1] = FormFx.HoldMark(FormMark.Cross, FormKit.Lit(cast.Colour, 3.2f));
            marks[2] = FormFx.HoldMark(FormMark.Brackets, FormKit.Lit(cast.Colour, 2.4f));
            Vector3 chest = victim.transform.position + Vector3.up * 1.1f;
            // Burned on: a cut across them and a ring snapping shut.
            FormFx.Cut(chest, Vector3.Cross(Vector3.up, cast.Forward) + cast.Forward * 0.4f, 4.4f, 0.5f, FormKit.Lit(cast.Colour, 3.4f), 0.18f);
            FormFx.Lay(FormMark.Ring, victim.transform.position, 5f, FormKit.Lit(cast.Colour, 3f), 0.22f, 1.8f);
            if (cast.Upgrades != null) cast.Upgrades.Jolt(0.03f, 0.12f, 0.15f);
            SpellWorks.Gesture(cast, FormKit.Cast, 0.3f);
            return 0;
        }

        /// <summary>Metres from the enemy a seal is slapped on from.</summary>
        public const float ArmsLength = 1.2f;

        /// <summary>The blister's sizes: 0 small, 1 middle, 2 large.</summary>
        public const int LargestBlister = 2;
        /// <summary>What a condition on the blistered enemy is, when breaking it on its poison's beat doubles the burst (Blister's Master).</summary>
        public const string DoublesOnBeat = "blister.doubles";
        /// <summary>Seconds from a bite of its poison a heavy blow counts as on the beat.</summary>
        public const float BeatWindow = 0.2f;

        /// <summary>
        /// The blister's burst at a size: half, the whole and 1.75 times the cast's middle damage, the largest rounded down to a
        /// five (20, 40, 70; with the Empower 25, 50, 85).
        /// </summary>
        public static float BlisterDamage(float middle, int size) =>
            size <= 0 ? middle * 0.5f : size == 1 ? middle : Mathf.Floor(middle * 1.75f / 5f + 0.0001f) * 5f;
        /// <summary>Its reach at a size: a metre less, the middle reach, a metre more (2, 3, 4 m).</summary>
        public static float BlisterRadius(float middle, int size) => Mathf.Max(0.5f, middle + Mathf.Clamp(size, 0, LargestBlister) - 1f);
        /// <summary>The doses it lays on everyone else it reaches at a size: half, the whole and half again the middle's (2, 4, 6).</summary>
        public static int BlisterDoses(int middle, int size) => Mathf.CeilToInt(middle * (Mathf.Clamp(size, 0, LargestBlister) + 1) * 0.5f);
        /// <summary>The size after a bite: one larger, never past the largest.</summary>
        public static int Swell(int size) => Mathf.Min(LargestBlister, size + 1);

        // The blister breaks at its size: the burst round it, the doses on everyone else it reaches, doubled on the beat with the Master.
        private static void BreakBlister(in SpellCast cast, Component victim, int size)
        {
            Vector3 foot = victim.transform.position;
            var set = victim is Clover.Gameplay.IAfflictable afflicted ? afflicted.Afflictions : null;
            bool doubled = set != null && set.Conditions.Holds(DoublesOnBeat) && set.NearBeat(BeatWindow);
            float scale = doubled ? 2f : 1f;
            float radius = BlisterRadius(cast.Spec.Radius, size);
            cast.Combat.StrikeArea(foot, radius, cast.Blow(BlisterDamage(cast.Damage, size) * scale, cast.Stagger * scale, 0));
            int doses = BlisterDoses(cast.Spec.Stacks, size) * (doubled ? 2 : 1);
            if (doses > 0) cast.Combat.AfflictArea(foot, radius, cast.Affliction, doses, victim, Clover.Gameplay.LaySource.Spell);
            if (doubled) BlistersDoubled++;
            FormKit.Impact(cast, foot, radius * 0.75f, 0.9f + 0.3f * size);
            FormFx.Face(FormMark.Cross, foot + Vector3.up * 1.1f, 2f + size, FormKit.Lit(cast.Colour, 3.6f), 0.14f, 4.4f);
        }

        /// <summary>Blisters broken on the beat for twice their burst (Blister's Master): for probes.</summary>
        public static int BlistersDoubled { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCounts() => BlistersDoubled = 0;

        // The seal's thunder (the ledger's seal-thunder): the cast's damage and so much a dose the enemy carries, all its
        // Static discharged into the same blow, the stagger in the seal's radius, and each dose passed to every enemy near.
        private static void Thunder(in SpellCast cast, Component victim, bool ranOut)
        {
            Vector3 foot = victim.transform.position;
            var set = victim is Clover.Gameplay.IAfflictable afflicted ? afflicted.Afflictions : null;
            int doses = set != null ? set.Stacks(cast.Affliction) : 0;
            int charge = set != null ? set.Consume(Clover.Gameplay.AfflictionKind.Static) : 0;
            var upgrades = cast.Upgrades;
            float perCharge = upgrades != null ? upgrades.StaticDamagePerStack : 7f, chargeStagger = upgrades != null ? upgrades.StaticStaggerPerStack : 5f;
            float damage = SealRules.Thunder(cast.Damage, cast.Spec.PerDose, doses, charge, perCharge, ranOut);
            float stagger = SealRules.Share(ranOut) * (cast.Stagger + chargeStagger * charge);
            cast.Combat.StrikeArea(foot, cast.Spec.Radius, cast.Blow(damage, stagger));
            if (doses > 0 && cast.Spec.PassReach > 0f)
                cast.Combat.AfflictArea(foot, cast.Spec.PassReach, cast.Affliction, doses, victim, Clover.Gameplay.LaySource.Spell);
            if (charge > 0 && upgrades != null) upgrades.Discharged(victim, charge);
            Clover.World.LightningBolts.Strike(foot, ranOut ? 0.6f : 1f);
            FormKit.Impact(cast, foot, cast.Spec.Radius * 0.75f, ranOut ? 0.8f : 1.3f);
            FormFx.Face(FormMark.Cross, foot + Vector3.up * 1.1f, 3f, FormKit.Lit(cast.Colour, 3.6f), 0.14f, 4.4f);
        }

        public override void Struck(ref SpellRun run, AttackStep step, Component target, float damage)
        {
            // A blister breaks only on a heavy blow.
            if (run.Cast.Spec.Swells && (step == null || !step.Heavy)) return;
            // Counted here, set off in the next step: the swing is still being reckoned.
            if (target != null && ReferenceEquals(target, run.Victim)) run.Phase++;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var marks = SpellWorks.Marks(run);
            if (!FormKit.Alive(run.Victim)) return false;
            Vector3 foot = run.Victim.transform.position;
            bool seal = cast.Spec.Seal;
            if (cast.Spec.Swells)
            {
                // It swells a size at each bite of its bearer's poison (the size kept in T1), and a heavy blow breaks it.
                int size = Mathf.RoundToInt(run.T1);
                if (size < LargestBlister && run.Victim is Clover.Gameplay.IAfflictable swelling && swelling.Afflictions.BitThisTick)
                {
                    size = Swell(size);
                    run.T1 = size;
                    FormFx.Lay(FormMark.Ring, foot, 1.5f + size, FormKit.Lit(cast.Colour, 2.6f), 0.2f, 1.4f);
                }
                if (run.Phase > 0)
                {
                    BreakBlister(cast, run.Victim, size);
                    return false;
                }
                // Left too long, it withers.
                if (run.Age >= Mathf.Max(0.5f, cast.Spec.Seconds)) return false;
                float grown = 0.6f + 0.35f * size;
                FormFx.PlaceMark(marks[0], foot, Quaternion.Euler(0f, run.Age * 80f, 0f), new Vector3(2.6f, 1f, 2.6f) * grown);
                FormFx.PlaceMarkFacing(marks[1], foot + Vector3.up * 2.9f, grown * (1f + 0.12f * Mathf.Sin(run.Age * 9f)), 0f);
                FormFx.PlaceMark(marks[2], foot, Quaternion.Euler(0f, -run.Age * 140f, 0f), new Vector3(1.7f, 1f, 1.7f) * grown);
                return true;
            }
            while (run.Phase > 0 && run.Done < run.Count)
            {
                run.Phase--;
                run.Done++;
                if (seal)
                {
                    // Each blow only chars the seal down; the last brings the thunder.
                    FormFx.Face(FormMark.Cross, foot + Vector3.up * 2.4f, 1.2f, FormKit.Lit(cast.Colour, 2.6f), 0.12f, 2.2f, run.Done * 30f);
                    if (run.Done == run.Count) Thunder(cast, run.Victim, false);
                    continue;
                }
                cast.Combat.StrikeArea(foot, cast.Spec.Radius, cast.Blow(cast.Damage, cast.Stagger));
                FormKit.Impact(cast, foot, cast.Spec.Radius * 0.75f, run.Done == run.Count ? 1.2f : 0.7f);
                FormFx.Face(FormMark.Cross, foot + Vector3.up * 1.1f, 3f, FormKit.Lit(cast.Colour, 3.6f), 0.14f, 4.4f, run.Done * 30f);
            }
            run.Phase = 0;
            if (run.Done >= run.Count) return false;
            if (run.Age >= Mathf.Max(0.5f, cast.Spec.Seconds))
            {
                // Its time ran out before the last blow: the thunder still falls, at half.
                if (seal) Thunder(cast, run.Victim, true);
                return false;
            }
            // The sign over the head beats, and is smaller for every burst spent.
            float left = 1f - run.Done / (float)run.Count;
            float beat = 1f + 0.12f * Mathf.Sin(run.Age * 9f);
            float snap = Mathf.Clamp01(run.Age / 0.1f);
            FormFx.PlaceMark(marks[0], foot, Quaternion.Euler(0f, run.Age * 80f, 0f), new Vector3(2.6f, 1f, 2.6f) * (2f - snap));
            FormFx.PlaceMarkFacing(marks[1], foot + Vector3.up * 2.9f, (0.7f + 0.7f * left) * beat, 0f);
            FormFx.PlaceMark(marks[2], foot, Quaternion.Euler(0f, -run.Age * 140f, 0f), new Vector3(1.7f, 1f, 1.7f));
            return true;
        }
    }
}

using Clover.Gameplay;
using Clover.World;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Plasma's Ball Lightning. A slow ball of white fire leaves the hand and drifts after the nearest enemy. A blow of
    /// the weapon that reaches it sends it fast along the swing and grows it a step, three at most, a ring round it for
    /// each. It bursts on the first enemy it meets, with a burn and Static both; left alone its time runs out and it bursts small.
    /// Reads: Speed (the drift), Seconds, Radius, Damage, Stagger, Stacks, LingerDamage (damage added for each strike).
    /// </summary>
    public class BallForm : RunningForm
    {
        public override string Id => SpellForms.Ball;

        public const int MostStrikes = 3;
        private const float Size = 0.5f;
        private const float BattedSpeed = 9f;
        private const float RadiusEach = 0.7f;
        private const float StaggerEach = 10f;
        private const float Meets = 1.05f;

        /// <summary>What the burst deals after some strikes: the spell's own and a set amount for each strike, three at most.</summary>
        public static float Burst(float damage, float each, int strikes) => damage + each * Mathf.Clamp(strikes, 0, MostStrikes);

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.A = cast.Origin + cast.Forward * 1.5f;
            Color white = Color.Lerp(cast.Colour, Color.white, 0.65f);
            var solids = SpellWorks.Solids(run);
            solids[0] = FormFx.Hold(FormMesh.Orb, white, 0.05f);
            var marks = SpellWorks.Marks(run);
            marks[0] = FormFx.HoldMark(FormMark.Star, FormKit.Lit(white, 3.2f));
            marks[1] = FormFx.HoldMark(FormMark.Disc, FormKit.Lit(cast.Colour, 1.5f));
            FormKit.Gather(cast, run.A, 1.6f);
            SpellWorks.Gesture(cast, FormKit.Cast, 0.3f);
            return 0;
        }

        public override void Swung(ref SpellRun run, AttackStep step, Vector3 origin, Vector3 forward)
        {
            if (step == null) return;
            Vector3 gap = run.A - origin;
            gap.y = 0f;
            if (gap.magnitude > step.Reach + 0.9f) return;
            if (gap.magnitude > 1f && Vector3.Angle(forward, gap) > step.ArcDegrees * 0.5f + 25f) return;
            forward.y = 0f;
            run.B = forward.sqrMagnitude > 0.001f ? forward.normalized : gap.normalized;
            run.Flag = true;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var marks = SpellWorks.Marks(run);
            Color white = Color.Lerp(cast.Colour, Color.white, 0.65f);
            float seconds = Mathf.Max(0.5f, cast.Spec.Seconds);
            if (run.Flag)
            {
                // Batted: off along the swing, a step larger, with another ring.
                run.Flag = false;
                run.Flag2 = true;
                if (run.Count < MostStrikes)
                {
                    run.Count++;
                    marks[1 + run.Count] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(white, 3f));
                }
                run.T2 = run.Age;
                FormFx.Face(FormMark.Star, run.A + Vector3.up * 1.2f, 2f, FormKit.Lit(white, 4f), 0.1f, 3.4f);
                FormFx.Run(FormMark.Streak, run.A, run.B, 5f, 1f, FormKit.Lit(white, 3.4f), 0.2f, 0.2f);
                LightningBolts.Strike(run.A, 0.25f);
                if (cast.Upgrades != null) cast.Upgrades.Jolt(0.05f, 0.25f, 0.15f);
            }
            if (run.Flag2) run.A += run.B * (BattedSpeed * deltaTime);
            else
            {
                run.T1 -= deltaTime;
                if (run.T1 <= 0f)
                {
                    run.T1 = 0.25f;
                    run.Victim = cast.Combat.EnemyNear(run.A, 16f, null);
                }
                if (FormKit.Alive(run.Victim))
                {
                    Vector3 gap = run.Victim.transform.position - run.A;
                    gap.y = 0f;
                    if (gap.magnitude > 0.5f) run.A += gap.normalized * (cast.Spec.Speed * deltaTime);
                }
            }
            // Does it meet anyone? Asked a few times in every metre it travels.
            run.T0 -= deltaTime;
            bool met = false;
            if (run.T0 <= 0f && run.Age > 0.3f)
            {
                run.T0 = run.Flag2 ? 0.03f : 0.1f;
                met = cast.Combat.EnemyNear(run.A, Meets + 0.15f * run.Count, null) != null;
            }
            bool spent = run.Age >= seconds || (run.Flag2 && run.Age > run.T2 + 1.6f);
            if (met || spent)
            {
                int strikes = run.Count;
                float radius = cast.Spec.Radius + RadiusEach * strikes;
                var blow = cast.Blow(Burst(cast.Damage, cast.Spec.LingerDamage, strikes), cast.Stagger + StaggerEach * strikes, 0);
                blow.Heavy = strikes >= MostStrikes;
                LastStruck = cast.Combat.StrikeArea(run.A, radius, blow);
                LastStrikes = strikes;
                cast.Combat.AfflictArea(run.A, radius, AfflictionKind.Burn, cast.Spec.Stacks + strikes);
                cast.Combat.AfflictArea(run.A, radius, AfflictionKind.Static, cast.Spec.Stacks + strikes);
                FormKit.Impact(cast, run.A, radius * 0.85f, 1f + 0.4f * strikes);
                LightningBolts.Strike(run.A, 0.4f + 0.2f * strikes);
                FormFx.Lay(FormMark.Ring, run.A, 1f, FormKit.Lit(white, 4f), 0.4f, radius * 2.4f);
                int thrown = 8 + 3 * strikes;
                for (int i = 0; i < thrown; i++)
                {
                    Vector3 outward = FormRules.OnRing(i, thrown, 1f, run.Age * 40f);
                    Vector3 from = run.A + Vector3.up * 1.2f;
                    FormFx.Fly(FormMesh.Shard, from, from + outward * radius * 1.15f, Quaternion.LookRotation(outward), Vector3.one * (1.2f + 0.25f * strikes), i % 2 == 0 ? white : cast.Colour, 0.24f);
                }
                if (strikes > 0) SigKit.Moment(cast, 0.05f * strikes, 0.25f * strikes, strikes >= MostStrikes ? 0.3f : 0f, 0.4f, 0.12f * strikes);
                return false;
            }
            float size = Size * (1f + 0.45f * run.Count) * Mathf.Clamp01(run.Age / 0.15f);
            Vector3 heart = run.A + Vector3.up * (1.25f + 0.1f * Mathf.Sin(run.Age * 3f));
            FormFx.Place(solids[0], heart, Quaternion.Euler(run.Age * 90f, run.Age * 130f, 0f), Vector3.one * size);
            FormFx.Heat(solids[0], 0.5f + 0.4f * Mathf.Sin(run.Age * 14f));
            FormFx.PlaceMarkFacing(marks[0], heart, size * 4.2f, run.Age * 120f);
            float across = (cast.Spec.Radius + RadiusEach * run.Count) * 2f;
            FormFx.PlaceMark(marks[1], run.A, Quaternion.identity, new Vector3(across, 1f, across));
            for (int i = 1; i <= run.Count; i++)
                FormFx.PlaceMarkFacing(marks[1 + i], heart, size * (2.2f + 0.9f * i), 0f);
            return true;
        }

        /// <summary>How many the last ball struck when it burst and how many times it had been batted, for probes.</summary>
        public static int LastStruck { get; private set; }
        public static int LastStrikes { get; private set; }

        // Domain reload is off: numbers from the last play must not be read in this one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            LastStruck = 0;
            LastStrikes = 0;
        }
    }
}

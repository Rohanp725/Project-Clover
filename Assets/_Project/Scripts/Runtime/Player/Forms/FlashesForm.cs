using Clover.Gameplay;
using Clover.World;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Lightning's Seven Flashes. The player is gone; lines of light are drawn on the ground from enemy to enemy
    /// (against one enemy, through it from seven sides); the player stands again where they began with their back to
    /// the fight, and holds still; then every line flares and the cuts land together, and any enemy left at full
    /// Static discharges. Untouchable while gone, open while still: struck then, the cuts land at half and nothing discharges.
    /// Reads: Count (cuts), Interval, Delay (the stillness), Damage (each cut), Stagger, Stacks, Reach.
    /// </summary>
    public class FlashesForm : RunningForm
    {
        public override string Id => SpellForms.Flashes;

        private const float AfterSeconds = 0.25f;
        private const float DischargeDamage = 7f;
        private const float DischargeStagger = 5f;
        private const float DischargeReach = 4f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            if (!SpellWorks.TakeBody(ref run, true, false))
            {
                run.Phase = 9;
                return 0;
            }
            run.Count = Mathf.Clamp(cast.Spec.Count, 1, 12);
            // Who is cut: everyone near, as many as there are cuts. T3 keeps how many.
            run.T3 = SigKit.Gather(run, cast.Origin, cast.Reach, Mathf.Min(run.Count, SpellWorks.NotesEach));
            run.C = cast.Origin;
            run.A = cast.Origin;
            run.B = cast.Forward;
            run.T0 = Mathf.Max(0.04f, cast.Spec.Interval);
            // No stillness at all when the cast was quickened (its Master, cast in a perfect dodge's moment).
            run.T1 = StillFor(cast.Spec.Delay);
            SpellWorks.Ghost(run.Count * run.T0 + 0.3f, FormKit.Lit(cast.Colour, 2.6f, 0.85f));
            cast.Player.Hidden = true;
            FormFx.Lay(FormMark.Ring, cast.Origin, 0.6f, FormKit.Lit(cast.Colour, 3f), 0.2f, 3f);
            FormKit.Flash(cast, 0.14f, 0.05f);
            return 0;
        }

        // Struck while standing still: the cuts are halved and the form goes on without the body.
        public override bool BodyLost(ref SpellRun run)
        {
            if (run.Phase != 1) return false;
            run.Flag = true;
            return true;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            if (run.Phase == 9) return false;
            var cast = run.Cast;
            var player = cast.Player;
            var things = SpellWorks.Things(run);
            int enemies = Mathf.RoundToInt(run.T3);
            Color thin = FormKit.Lit(cast.Colour, 3.2f);
            if (run.Phase == 0)
            {
                while (run.Done < run.Count && run.Age >= 0.08f + run.Done * run.T0)
                {
                    int k = run.Done++;
                    float lasts = (run.Count - k) * run.T0 + run.T1 + 0.06f;
                    Vector3 from = run.A, to;
                    if (enemies <= 0) to = run.C + Quaternion.Euler(0f, k * 51f, 0f) * run.B * 4f;
                    else
                    {
                        var cut = things[k % enemies];
                        Vector3 at = FormKit.Alive(cut) ? SigKit.Foot(cut, run.C.y) : run.A;
                        // Through it and out the far side; against one enemy each line comes from another side, a star.
                        Vector3 way = at - from;
                        if (enemies == 1 || way.sqrMagnitude < 0.3f)
                        {
                            way = Quaternion.Euler(0f, k * 154f, 0f) * run.B;
                            from = at - way.normalized * 3.2f;
                        }
                        to = at + way.normalized * 2.2f;
                    }
                    Vector3 line = to - from;
                    FormFx.Run(FormMark.Streak, from, line, line.magnitude, 0.3f, thin, lasts, 0.22f);
                    FormFx.Run(FormMark.Streak, from, line, line.magnitude * 1.1f, 1.1f, FormKit.Lit(cast.Colour, 4f), 0.08f, 0.15f);
                    ElementLook.Sparks(cast.Element, to + Vector3.up, line, 0.5f);
                    if (cast.Upgrades != null) cast.Upgrades.Jolt(0f, 0.07f, 0.08f);
                    run.A = to;
                }
                if (run.Age < 0.08f + run.Count * run.T0) return true;
                // Back where they began, their back to the fight. From here they can be struck.
                run.Phase = 1;
                run.T2 = run.Age;
                player.Hidden = false;
                player.StepTo(run.C);
                SpellWorks.GiveBody(ref run);
                // Quickened, the player is free at once and the cuts land on the next step.
                if (run.T1 <= 0f) return true;
                Vector3 away = enemies > 0 && FormKit.Alive(things[0]) ? run.C - SigKit.Foot(things[0], run.C.y) : -run.B;
                if (away.sqrMagnitude > 0.01f) player.transform.rotation = Quaternion.LookRotation(away.normalized, Vector3.up);
                SpellWorks.TakeBody(ref run, false, false);
                FormFx.Lay(FormMark.Ring, run.C, 2.6f, FormKit.Lit(cast.Colour, 3f), 0.2f, 0.8f);
                return true;
            }
            if (run.HoldsBody)
            {
                player.FormPose = FormKit.Draw;
                player.FormSwordOut = true;
                player.FormPoseProgress = run.Phase == 1 ? Mathf.Lerp(0.62f, 0.9f, (run.Age - run.T2) / run.T1) : Mathf.Lerp(0.9f, 0.99f, (run.Age - run.T2) / AfterSeconds);
            }
            if (run.Phase == 1)
            {
                if (run.Age < run.T2 + run.T1) return true;
                run.Phase = 2;
                run.T2 = run.Age;
                float share = run.Flag ? 0.5f : 1f;
                int landed = 0;
                for (int k = 0; k < run.Count && enemies > 0; k++)
                {
                    var cut = things[k % enemies];
                    if (!FormKit.Alive(cut)) continue;
                    Vector3 chest = cut.transform.position + Vector3.up * 1.1f;
                    landed += cast.Combat.StrikeOne(cut, cast.Blow(cast.Damage * share, cast.Stagger * share), run.C) ? 1 : 0;
                    FormFx.Cut(chest, Quaternion.Euler(0f, k * 154f, 0f) * run.B, 6f, 0.8f, FormKit.Lit(cast.Colour, 4f), 0.16f, 0.05f);
                }
                for (int i = 0; i < enemies; i++)
                {
                    if (!FormKit.Alive(things[i])) continue;
                    Vector3 at = SigKit.Foot(things[i], run.C.y);
                    FormFx.Face(FormMark.Star, at + Vector3.up * 1.1f, 3f, FormKit.Lit(cast.Colour, 4f), 0.11f, 4.4f);
                    FormFx.Lay(FormMark.Cross, at, 4f, FormKit.Lit(cast.Colour, 2.6f), 1f, -1f, 0f, 30f + i * 40f);
                    // Whoever the cuts filled goes off, unless the player was struck while still.
                    var set = SigKit.Set(things[i]);
                    if (run.Flag || set == null || set.Stacks(AfflictionKind.Static) < Afflictions.Depth(AfflictionKind.Static)) continue;
                    int stacks = set.Consume(AfflictionKind.Static);
                    LightningBolts.Strike(at, 0.5f);
                    cast.Combat.StrikeArea(at, DischargeReach, cast.Blow(DischargeDamage * stacks, DischargeStagger * stacks, 0));
                    run.Done++;
                }
                Discharged = run.Done - run.Count;
                if (landed > 0) SigKit.Moment(cast, run.Flag ? 0.06f : 0.17f, 0.55f, run.Flag ? 0f : 0.3f, 0.4f, 0.34f);
                return true;
            }
            return run.Age < run.T2 + AfterSeconds;
        }

        /// <summary>The seconds the player stands still before the cuts land: none for a quickened cast (Delay 0), else at least 0.15.</summary>
        public static float StillFor(float delay) => delay <= 0f ? 0f : Mathf.Max(0.15f, delay);

        /// <summary>How many enemies the last cast discharged, for probes.</summary>
        public static int Discharged { get; private set; }

        // Domain reload is off: a number from the last play must not be read in this one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Discharged = 0;
    }
}

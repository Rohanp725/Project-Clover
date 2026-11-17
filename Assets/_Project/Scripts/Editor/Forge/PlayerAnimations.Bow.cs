using System;
using System.Collections.Generic;
using Clover.Player;
using UnityEngine;

namespace Clover.EditorTools.Forge
{
    /// <summary>
    /// The Great Bow's clips: the only set whose weapon is in the left hand (<see cref="ForgePose.AimBladeL"/> points
    /// the upper limb, <see cref="ForgePose.BladeLBack"/> says which way the string faces), with an arrow in the right.
    /// An upright bow is a dot from the game's camera, so every pose cants it: the stave lies out to the left of the
    /// bow hand like a horse archer's, where it reads as a long bar ahead of the hat with the arrow a line across it.
    /// </summary>
    public static partial class PlayerAnimations
    {
        private static IEnumerable<ForgeClip> BowClips()
        {
            var combo = Weapons.GreatBow().Combo;
            yield return BowIdle();
            yield return BowRun();
            yield return BowGuard();
            yield return BowGuardHit();
            yield return BowGuardPerfect();
            yield return BowCharge(combo.ChargeTime);
            yield return BowSwat(combo.Steps[0]);
            yield return BowStab(combo.Steps[1]);
            yield return BowSnapShot(combo.Heavy);
            yield return BowKickShot(combo.ChainHeavy);
            yield return BowFullDraw(combo.ChargedHeavy);
            yield return BowLandmines(combo.Skill);
            yield return BowNailed(combo.Finisher);
        }

        // A pose with the bow in the left hand and an arrow in the right: where the bow hand is, where the upper limb
        // points and which way the string faces; where the arrow hand is and where the arrow points.
        private static ForgePose Archer(Vector3 hips, float hipYaw, float waistYaw, float lean, Vector3 footL, Vector3 footR,
            Vector3 bowHand, Vector3 limb, Vector3 stringSide, Vector3 arrowHand, Vector3 arrow, float keep = 0.85f)
        {
            return Looking(new ForgePose
            {
                Hips = hips,
                HipYaw = hipYaw,
                HipLean = lean * 0.4f,
                WaistYaw = waistYaw,
                WaistLean = lean * 0.6f,
                FootL = footL,
                FootR = footR,
                FootLTurn = 6f,
                FootRTurn = 50f,
                HandL = bowHand,
                HandR = arrowHand,
                ElbowL = new Vector3(-1f, -0.4f, -0.2f),
                ElbowR = new Vector3(1f, -0.2f, -0.6f),
                AimBlade = true,
                Blade = arrow,
                AimBladeL = true,
                BladeL = limb,
                BladeLBack = stringSide,
            }, keep);
        }

        private static readonly Vector3 BowFootL = new Vector3(-0.1f, 0f, 0.3f), BowFootR = new Vector3(0.2f, 0f, -0.28f);
        // The stave canted out to the left and a little forward: the line the camera sees.
        private static readonly Vector3 BowCant = new Vector3(-0.72f, 0.66f, 0.12f), StringBack = new Vector3(0f, 0f, -1f);

        /// <summary>Tall and side on, left side to the enemy: the bow canted out from the left hand, the arrow held low across the thighs.</summary>
        private static ForgePose BowReady() => Archer(V(0f, -0.06f, 0f), 52f, 10f, 2f, BowFootL, BowFootR,
            V(-0.44f, 1.04f, 0.46f), V(-0.5f, 0.84f, 0.2f), V(0.3f, 0f, -1f), V(0.34f, 0.86f, 0.12f), V(-0.72f, -0.12f, 0.68f));

        private static ForgeClip BowIdle() => new ForgeClip("idle_bow", 2.4f, true).From(t =>
        {
            // Still, but for the breath and the head: it turns slowly from side to side, scanning.
            float breath = Mathf.Sin(t * Mathf.PI * 2f);
            var p = BowReady();
            p.Hips += V(0f, 0.012f * breath, 0f);
            p.HandL += V(0f, 0.014f * breath, 0f);
            p.HandR += V(0f, 0.008f * breath, 0f);
            p.HeadYaw += 22f * Mathf.Sin(t * Mathf.PI * 2f + 0.8f);
            return p;
        });

        /// <summary>The bow carried level at the left hip, pointing ahead like a lance and lying flat; the arrow hand pumping.</summary>
        private static ForgeClip BowRun()
        {
            var clip = Renamed(RunFist(), "run_bow");
            clip.Seconds = 0.56f;
            Func<float, ForgePose, ForgePose> carried = (t, p) =>
            {
                float sway = Mathf.Sin(t * Mathf.PI * 2f);
                p.WaistLean += 6f;
                p.HandL = V(-0.46f, 0.98f + 0.02f * sway, 0.3f);
                p.ElbowL = V(-1f, -0.2f, -0.6f);
                p.AimBladeL = true;
                p.BladeL = V(-0.16f, 0.12f, 1f);
                p.BladeLBack = V(1f, 0.25f, 0f);
                p.HandR = V(0.4f, 1.0f - 0.07f * sway, 0.08f + 0.2f * sway);
                p.AimBlade = true;
                p.Blade = V(0.2f, -0.3f, 0.93f);
                p.TwoHanded = false;
                return p;
            };
            clip.Shape = clip.Shape != null ? Both(clip.Shape, carried) : carried;
            return clip;
        }

        // The stave slantwise across the front in both hands, belly to the enemy, body low behind it.
        private static ForgePose BowBarred() => With(Archer(V(0f, -0.26f, -0.04f), 16f, 0f, 10f, V(-0.2f, 0f, 0.24f), V(0.22f, 0f, -0.2f),
            V(-0.36f, 1.0f, 0.52f), V(0.62f, 0.76f, 0.16f), V(0f, 0.1f, -1f), V(0.12f, 1.58f, 0.62f), V(0.62f, 0.76f, 0.16f), 0.6f), p =>
        {
            p.ElbowR = V(1f, 0.2f, 0f);
            p.ElbowL = V(-1f, -0.5f, 0f);
            p.ShoulderRUp = 14f;
        });

        private static ForgeClip BowGuard() => new ForgeClip("guard_bow", 0.14f).Key(0f, BowReady()).Key(1f, BowBarred(), Ease.Out);

        private static ForgeClip BowGuardHit()
        {
            // The stave flexes (the tips nod back as the hands are driven in) and the bearer gives a step.
            var driven = With(BowBarred(), p =>
            {
                p.Hips = V(0f, -0.32f, -0.24f);
                p.HandL = V(-0.3f, 1.02f, 0.3f);
                p.HandR = V(0.14f, 1.52f, 0.36f);
                p.BladeL = V(0.6f, 0.7f, -0.2f);
                p.FootL += V(0f, 0f, -0.16f);
                p.FootR += V(0f, 0f, -0.16f);
                p.WaistLean = -6f;
            });
            return new ForgeClip("guard_hit_bow", 0.22f).Key(0f, BowBarred()).Key(0.25f, driven, Ease.Snap).Key(1f, BowBarred(), Ease.InOut);
        }

        /// <summary>The lower tip hooks the blow down and aside to the left; the right hand, arrow held like a dagger, punches in over the top.</summary>
        private static ForgeClip BowGuardPerfect()
        {
            var hooked = With(Archer(V(0.06f, -0.3f, 0.14f), -14f, -14f, 12f, V(-0.2f, 0f, 0.34f), V(0.24f, 0f, -0.22f),
                V(-0.74f, 0.98f, 0.56f), V(0.84f, 0.44f, -0.3f), V(0.3f, 0f, -0.95f), V(0.18f, 1.46f, 1.06f), V(0f, -0.4f, 0.92f)), p =>
            {
                p.ElbowR = V(1f, 0.4f, -0.2f);
                p.ShoulderRForward = 30f;
                p.FootRTip = 30f;
            });
            var empty = With(hooked, p =>
            {
                p.HandR = V(0.36f, 1.2f, 0.3f);
                p.Blade = V(0.2f, -0.6f, 0.75f);
                p.Hips = V(0f, -0.2f, 0f);
            });
            return new ForgeClip("guard_perfect_bow", 0.3f).Key(0f, BowBarred()).Key(0.3f, hooked, Ease.Snap).Key(0.6f, hooked, Ease.Linear).Key(1f, empty, Ease.InOut);
        }

        // Both hands high above the head, arrow nocked: where the high draw begins.
        private static ForgePose BowRaised() => With(Archer(V(0f, -0.02f, 0f), 66f, 12f, -6f, V(-0.12f, 0f, 0.36f), V(0.22f, 0f, -0.36f),
            V(-0.12f, 2.02f, 0.5f), V(-0.8f, 0.5f, 0.2f), V(0.2f, 0.3f, -0.9f), V(0.0f, 2.0f, 0.3f), V(-0.1f, 0f, 1f)), p =>
        {
            p.ShoulderLUp = p.ShoulderRUp = 30f;
            p.ElbowR = V(1f, 0f, -0.3f);
        });

        // The full draw: the bow arm straight at the enemy, the string hand behind the ear, hips square and feet wide.
        private static ForgePose BowDrawn(float draw = 1f) => With(Archer(V(0f, -0.16f, -0.04f), 70f, 14f, -4f, V(-0.14f, 0f, 0.46f), V(0.24f, 0f, -0.46f),
            V(-0.1f, 1.5f, 0.92f), BowCant, StringBack, V(0.06f, 1.56f, Mathf.Lerp(0.5f, -0.24f, draw)), V(-0.14f, -0.05f, 1f)), p =>
        {
            p.ElbowR = V(0.5f, 0.3f, -1f);
            p.ElbowL = V(-1f, 0f, 0f);
            p.ShoulderRUp = 16f;
            p.ShoulderLForward = 20f;
        });

        private static ForgeClip BowCharge(float seconds)
        {
            var clip = new ForgeClip("charge_bow", seconds).Key(0f, BowReady()).Key(0.3f, BowRaised(), Ease.Out).Key(0.9f, BowDrawn(), Ease.InOut).Key(1f, BowDrawn(), Ease.Linear);
            // At the anchor the arms tremble.
            clip.Shape = (t, p) =>
            {
                if (t < 0.85f) return p;
                float shake = Mathf.Sin(t * 400f) * 0.006f;
                p.HandR += V(shake, -shake, 0f);
                p.HandL += V(-shake, shake, 0f);
                return p;
            };
            return clip;
        }

        /// <summary>
        /// The swat: the bow is drawn back across the body to the right hip, upper limb trailing, and swept out
        /// backhand at head height with the left arm straightening, the horn cap leading: a long thin arc, finer than a sword's.
        /// </summary>
        private static ForgeClip BowSwat(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var wound = With(Archer(V(0f, -0.2f, -0.06f), -30f, -30f, 10f, BowFootL, BowFootR,
                V(0.34f, 1.06f, 0.16f), V(0.62f, 0.3f, -0.72f), V(-0.7f, 0.2f, -0.7f), V(0.44f, 0.9f, -0.2f), V(0.1f, -0.9f, 0.3f)), p =>
            {
                p.ElbowL = V(-0.3f, -0.4f, 1f);
                p.ShoulderLForward = 30f;
            });
            var coming = With(Archer(V(0f, -0.2f, 0.04f), 0f, -6f, 10f, V(-0.12f, 0f, 0.36f), BowFootR,
                V(0.2f, 1.4f, 0.6f), V(0.94f, 0.2f, 0.28f), V(-0.3f, 0.2f, -0.9f), V(0.44f, 0.9f, -0.2f), V(0.1f, -0.9f, 0.3f)), p => p.ShoulderLForward = 26f);
            var struck = With(Archer(V(0f, -0.2f, 0.14f), 40f, 20f, 12f, V(-0.12f, 0f, 0.42f), BowFootR,
                V(-0.3f, 1.48f, 0.78f), V(-0.12f, 0.16f, 1f), V(0.9f, 0.2f, 0f), V(0.46f, 0.92f, -0.24f), V(0.1f, -0.95f, 0.2f)), p =>
            {
                p.ElbowL = V(-1f, 0f, 0f);
                p.FootRTip = 30f;
            });
            var carried = With(struck, p =>
            {
                p.HandL = V(-0.78f, 1.4f, 0.3f);
                p.BladeL = V(-0.92f, 0.16f, 0.36f);
                p.BladeLBack = V(0.3f, 0.2f, -0.9f);
                p.HipYaw = 60f;
                p.WaistYaw = 30f;
                Looking(p);
            });
            return Attack(step, -5f).Key(0f, BowReady()).Key(hit * 0.55f, wound, Ease.Out).Key(hit * 0.85f, coming, Ease.In).Key(hit, struck, Ease.Linear)
                .Key(done, carried, Ease.Out).Key(1f, With(carried, p => p.Hips += V(0f, 0.02f, -0.03f)), Ease.InOut);
        }

        /// <summary>The arrow stab: the right fist up by the ear with the arrow as a dagger, the bow held out of the way; a stride and an overhand stab; the hand comes away open and goes to the quiver.</summary>
        private static ForgeClip BowStab(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var raised = With(Archer(V(0f, -0.14f, -0.06f), 30f, 16f, -8f, BowFootL, BowFootR,
                V(-0.78f, 1.16f, 0.26f), V(-0.86f, 0.5f, 0.1f), V(0.3f, 0.2f, -0.9f), V(0.36f, 1.92f, -0.06f), V(0f, -0.35f, 0.94f)), p =>
            {
                p.ShoulderRUp = 30f;
                p.ElbowR = V(1f, 0.2f, -0.4f);
            });
            // The fist ends well ahead of the hat and the arrow slants on from it, so the stab is a line up the screen and not a hand under a brim.
            var driven = With(Archer(V(0f, -0.3f, 0.3f), -26f, -24f, 16f, V(-0.16f, 0f, 0.3f), V(0.16f, 0f, 0.5f),
                V(-0.86f, 1.1f, 0.0f), V(-0.94f, 0.3f, -0.2f), V(0.3f, 0.2f, -0.9f), V(0.16f, 1.34f, 1.14f), V(0f, -0.42f, 0.9f)), p =>
            {
                p.ShoulderRForward = 34f;
                p.ElbowR = V(1f, 0.5f, 0f);
                p.FootLTip = 30f;
            });
            var away = With(Archer(V(0f, -0.14f, 0.1f), 10f, 0f, 4f, V(-0.16f, 0f, 0.22f), V(0.18f, 0f, 0.3f),
                V(-0.6f, 1.06f, 0.3f), V(-0.6f, 0.78f, 0.16f), V(0.3f, 0f, -1f), V(0.42f, 0.9f, -0.12f), V(0.1f, -0.9f, -0.3f)), p => p.ElbowR = V(1f, 0f, -1f));
            return Attack(step, -6f).Key(0f, BowReady()).Key(hit * 0.6f, raised, Ease.Out).Key(hit * 0.8f, With(raised, p => p.HandR += V(0f, 0.04f, -0.04f)), Ease.Linear)
                .Key(hit, driven, Ease.In).Key(done, driven, Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.45f), away, Ease.Out).Key(1f, away, Ease.Linear);
        }

        // Loosed: the string hand flown back, the bow rocked forward in the left with its upper tip toward the target.
        private static ForgePose BowLoosed(float back, float height = 1.5f) => With(BowDrawn(), p =>
        {
            p.HandL = V(-0.1f, height, 0.96f);
            p.HandR = V(0.3f, height + 0.04f, -0.62f);
            p.ElbowR = V(0.4f, 0.4f, -1f);
            // The arrow still in the fist is the next one: it hangs point down, out of the way, so the hand reads as having let go.
            p.Blade = V(0.2f, -0.9f, 0.36f);
            p.BladeL = V(-0.6f, 0.45f, 0.66f);
            p.BladeLBack = V(0f, 0.5f, -0.86f);
            p.Hips = V(0f, -0.14f, -0.04f - back);
            p.WaistLean = -4f - back * 30f;
            p.FootRTip = 0f;
        });

        /// <summary>The snap shot: nock and half draw at chest height with no ceremony, a short held beat, loose; the body sways back and the right hand sweeps down to the quiver.</summary>
        private static ForgeClip BowSnapShot(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var half = With(BowDrawn(0.62f), p =>
            {
                p.HandL = V(-0.1f, 1.36f, 0.86f);
                p.HandR = V(0.06f, 1.4f, 0.02f);
                p.Hips = V(0f, -0.2f, -0.02f);
            });
            var quiver = With(BowLoosed(0.1f, 1.36f), p =>
            {
                p.HandR = V(0.44f, 0.94f, -0.2f);
                p.Blade = V(0.2f, -0.9f, -0.3f);
                p.BladeL = BowCant;
                p.BladeLBack = StringBack;
            });
            var clip = Attack(step, 0f);
            clip.Shape = null;
            return clip.Key(0f, BowReady()).Key(hit * 0.55f, half, Ease.Out).Key(hit * 0.97f, With(half, p => p.HandR += V(0f, 0f, -0.05f)), Ease.Linear)
                .Key(hit, BowLoosed(0f, 1.36f), Ease.Snap).Key(done, BowLoosed(0.1f, 1.36f), Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), quiver, Ease.InOut).Key(1f, quiver, Ease.Linear);
        }

        /// <summary>
        /// The kick shot: a skip in with the right knee lifted, a flat stamping kick to the chest; the kicking leg
        /// pushes off and the body goes back laid out in the air, where the bow is drawn across the chest and loosed
        /// down the line of the body; it lands on the left foot and right knee, already reaching for an arrow.
        /// </summary>
        private static ForgeClip BowKickShot(AttackStep step)
        {
            var (hit, done) = Moments(step);
            float total = step.Total;
            float loose = (step.Windup + step.Shot.ReleaseAfter) / total;
            var lifted = With(Archer(V(0f, -0.1f, 0.06f), 10f, 0f, -10f, V(-0.12f, 0f, 0.1f), V(0.12f, 0.62f, 0.3f),
                V(-0.6f, 1.2f, 0.2f), V(-0.8f, 0.6f, 0f), V(0.3f, 0f, -1f), V(0.44f, 1.1f, -0.1f), V(0.2f, -0.4f, 0.9f)), p => p.FootRTip = -10f);
            var kicked = With(Archer(V(0f, -0.06f, 0.1f), 16f, 0f, -26f, V(-0.12f, 0f, 0f), V(0.06f, 1.02f, 1.04f),
                V(-0.7f, 1.3f, -0.1f), V(-0.8f, 0.6f, -0.2f), V(0.3f, 0f, -1f), V(0.5f, 1.2f, -0.3f), V(0.2f, -0.2f, 0.95f)), p =>
            {
                p.FootRTip = -30f;
                p.FootRTurn = 0f;
                p.KneeR = V(0f, 1f, 0.3f);
            });
            // Laid back in the air, feet toward the enemy, the bow flat across the chest and drawn along the body's own line.
            var flying = With(Archer(V(0f, 0.22f, -0.1f), 40f, 10f, -52f, V(-0.14f, 0.5f, 0.5f), V(0.12f, 0.62f, 0.6f),
                V(-0.2f, 1.5f, 0.56f), V(-0.9f, 0.4f, 0.1f), V(0.2f, 0.6f, -0.8f), V(0.1f, 1.62f, -0.3f), V(-0.2f, -0.2f, 1f)), p =>
            {
                p.FootLTip = p.FootRTip = 40f;
                p.HeadLean = 30f;
                p.ElbowR = V(1f, 0.3f, -0.6f);
            });
            var loosed = With(flying, p =>
            {
                p.HandR = V(0.4f, 1.6f, -0.7f);
                p.Blade = V(0.4f, 0f, -0.9f);
                p.BladeL = V(-0.7f, 0.3f, 0.64f);
            });
            var landed = With(Archer(V(0f, -0.52f, 0f), 40f, 10f, 14f, V(-0.14f, 0f, 0.34f), V(0.22f, 0f, -0.5f),
                V(-0.4f, 0.9f, 0.5f), V(-0.2f, 0.2f, 1f), V(1f, 0.2f, 0f), V(0.44f, 0.72f, -0.2f), V(0.2f, -0.9f, -0.3f)), p =>
            {
                p.FootRTip = 70f;
                p.KneeR = V(0.3f, -0.6f, 1f);
            });
            var clip = Attack(step, 8f);
            clip.Shape = null;
            clip.SheetMoments = new[] { 0f, hit * 0.7f, hit, done, loose * 0.98f, loose, Mathf.Lerp(loose, 1f, 0.5f), 1f };
            return clip.Key(0f, BowReady()).Key(hit * 0.6f, lifted, Ease.Out).Key(hit, kicked, Ease.In).Key(done, kicked, Ease.Linear)
                .Key(Mathf.Lerp(done, loose, 0.7f), flying, Ease.Out).Key(loose * 0.99f, flying, Ease.Linear).Key(loose, loosed, Ease.Snap)
                .Key(Mathf.Lerp(loose, 1f, 0.45f), landed, Ease.In).Key(1f, landed, Ease.Linear);
        }

        /// <summary>The full draw, loosed: the bow kicks, the whole body is rocked back on the heels, and the bow arm stays out, pointing.</summary>
        private static ForgeClip BowFullDraw(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var clip = Attack(step, 0f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 1.3f);
            return clip.Key(0f, BowDrawn()).Key(hit * 0.9f, BowDrawn(), Ease.Linear).Key(hit, BowLoosed(0.06f), Ease.Snap).Key(done, BowLoosed(0.24f), Ease.Out)
                .Key(Mathf.Lerp(done, 1f, 0.66f), BowLoosed(0.2f), Ease.Linear).Key(1f, BowReady(), Ease.InOut);
        }

        /// <summary>Landmines: the bow tipped back and a handful of arrows loosed almost straight up, the body arched under it; then the bow is lowered without a look as they come down.</summary>
        private static ForgeClip BowLandmines(AttackStep step)
        {
            var (hit, done) = Moments(step);
            Vector3 up = V(0f, 0.92f, 0.4f);
            // Second pass: arched further back under the bow and drawn a hand longer, so the body is a bent bow itself.
            var tipped = With(Archer(V(0f, -0.18f, 0.16f), 60f, 10f, -36f, V(-0.14f, 0f, 0.46f), V(0.24f, 0f, -0.46f),
                V(-0.06f, 2.04f, 0.6f), V(-0.74f, 0.26f, -0.62f), -up, V(0.1f, 1.26f, 0.24f), up), p =>
            {
                p.ShoulderLUp = 34f;
                p.ElbowR = V(1f, -0.4f, -0.4f);
                p.HeadLean = -24f;
            });
            var loosed = With(tipped, p =>
            {
                p.HandR = V(0.4f, 1.0f, -0.1f);
                p.Blade = V(0.3f, -0.9f, 0f);
                p.HandL = V(-0.08f, 2.06f, 0.6f);
                p.WaistLean -= 8f;
            });
            var lowered = With(BowReady(), p => p.HeadLean = 14f);
            var clip = Attack(step, 0f);
            clip.Shape = null;
            return clip.Key(0f, BowDrawn()).Key(hit * 0.6f, tipped, Ease.Out).Key(hit * 0.96f, With(tipped, p => p.HandR += V(0f, -0.05f, -0.03f)), Ease.Linear)
                .Key(hit, loosed, Ease.Snap).Key(done, loosed, Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.6f), lowered, Ease.InOut).Key(1f, lowered, Ease.Linear);
        }

        /// <summary>
        /// "Nailed". The lower limb sweeps the kneeling enemy's legs and the upper, coming over, knocks it down; the
        /// left foot is set on it and the bow comes up overhead; then the high draw, made slowly and straight down
        /// until the arrow's head is a hand from its chest and the stave, lying across the whole body, is bent to its
        /// limit; loose; and the bearer straightens and looks away, drawing the next arrow.
        /// </summary>
        private static ForgeClip BowNailed(AttackStep step)
        {
            var (hit, done) = Moments(step);
            float total = step.Total;
            float first = step.Beats[0].Time / total, second = step.Beats[1].Time / total, hold = step.HoldAt / total;
            var sweep = With(Archer(V(0f, -0.4f, 0.1f), 30f, 20f, 24f, V(-0.16f, 0f, 0.34f), V(0.22f, 0f, -0.3f),
                V(-0.2f, 0.96f, 0.56f), V(-0.72f, 0.56f, -0.4f), V(0.5f, 0.2f, -0.8f), V(0.44f, 0.9f, -0.2f), V(0.1f, -0.9f, 0.3f)), p => p.FootRTip = 30f);
            var over = With(Archer(V(0f, -0.26f, 0.16f), -10f, -10f, 26f, V(-0.16f, 0f, 0.38f), V(0.22f, 0f, -0.26f),
                V(-0.16f, 1.2f, 0.84f), V(0.1f, -0.1f, 1f), V(0f, 1f, 0.1f), V(0.44f, 0.94f, -0.2f), V(0.1f, -0.9f, 0.3f)), p => p.FootRTip = 30f);
            // Down the line of the arrow: from the string hand high and back to the bow hand low and ahead.
            Vector3 down = V(-0.08f, -0.82f, 0.56f), across = V(-0.92f, 0.2f, 0.34f);
            // Second pass: the body sinks further onto the front knee as the draw fills, and the string hand goes higher.
            ForgePose Aimed(float draw, float sink) => With(Archer(V(0f, -0.1f - 0.24f * sink, 0.16f), 34f, 8f, 20f + 10f * sink, V(-0.06f, 0.3f, 0.72f), V(0.22f, 0f, -0.3f),
                V(-0.08f, Mathf.Lerp(1.5f, 1.16f, sink), 0.8f), across, -down, Vector3.Lerp(V(0f, 1.6f, 0.66f), V(0.04f, 2.08f, 0.2f), draw), down, 0.5f), p =>
            {
                p.KneeL = V(-0.3f, 0.4f, 1f);
                p.FootLTip = 10f;
                p.FootRTip = 36f;
                p.ElbowR = V(1f, 0.6f, -0.3f);
                p.ElbowL = V(-1f, 0.2f, 0.2f);
                p.ShoulderRUp = 34f * draw;
                p.HeadLean = 24f;
            });
            var overhead = With(Aimed(0f, 0f), p =>
            {
                p.HandL = V(-0.1f, 2.04f, 0.5f);
                p.HandR = V(0.04f, 2.0f, 0.3f);
                p.BladeL = V(-0.9f, 0.4f, 0.1f);
                p.BladeLBack = V(0f, 0.2f, -1f);
                p.Blade = V(0f, -0.1f, 1f);
                p.ShoulderLUp = p.ShoulderRUp = 30f;
                p.WaistLean = 0f;
            });
            var loosed = With(Aimed(1f, 1f), p =>
            {
                p.HandR = V(0.3f, 2.1f, -0.2f);
                p.Blade = V(0.3f, 0.6f, -0.7f);
                p.HandL += V(0f, 0.1f, 0.04f);
                p.WaistLean -= 24f;
                p.Hips += V(0f, 0.14f, -0.06f);
            });
            var away = With(Archer(V(0f, -0.04f, -0.1f), 150f, 20f, 0f, V(-0.1f, 0f, 0.1f), V(0.2f, 0f, -0.3f),
                V(-0.4f, 1.0f, -0.3f), V(0.5f, 0.84f, -0.2f), V(-0.3f, 0f, 1f), V(0.42f, 0.92f, -0.24f), V(0.1f, -0.9f, -0.3f), 0f), p => p.HeadLean = 10f);
            var clip = Attack(step, -4f);
            clip.Shape = (t, p) =>
            {
                // The arms tremble through the hold, more as the stave bends.
                if (t < hold || t >= hit) return p;
                float shake = Mathf.Sin(t * 520f) * 0.008f * Mathf.InverseLerp(hold, hit, t);
                p.HandR += V(shake, shake, 0f);
                p.HandL += V(-shake, 0f, shake);
                return p;
            };
            clip.SheetMoments = new[] { first, second, Mathf.Lerp(second, hold, 0.6f), hold, Mathf.Lerp(hold, hit, 0.6f), hit * 0.98f, hit, Mathf.Lerp(done, 1f, 0.6f) };
            return clip.Key(0f, BowReady()).Key(first, sweep, Ease.In).Key(second, over, Ease.In).Key(Mathf.Lerp(second, hold, 0.7f), overhead, Ease.Out)
                .Key(hold, Aimed(0.1f, 0.2f), Ease.InOut).Key(hit * 0.98f, Aimed(1f, 1f), Ease.Out).Key(hit, loosed, Ease.Snap).Key(done, loosed, Ease.Linear)
                .Key(Mathf.Lerp(done, 1f, 0.3f), loosed, Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.8f), away, Ease.InOut).Key(1f, away, Ease.Linear);
        }
    }
}
